using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MezeoFileSupport;
using System.Text.RegularExpressions;
using System.IO;

namespace Mezeo
{
    class FileDownloader
    {
        private static string DB_STATUS_SUCCESS = "SUCCESS";
        private static string DB_STATUS_IN_PROGRESS = "INPROGRESS";

        private static int INSUFFICIENT_STORAGE_AVAILABLE = -5;
        //private static int FILE_DOWNLOAD_SUCCESS = 200;

        Queue<LocalItemDetails> queue;
        ThreadLockObject lockObject;
        CloudService cFileCloud;
        DbHandler dbhandler = new DbHandler();
        public bool IsAnalysisCompleted { get; set; }

        public delegate void FileDownloadEvent(object sender, FileDownloaderEvents e);
        public event FileDownloadEvent downloadEvent;

        public delegate void FileDownloadCompletedEvent();
        public event FileDownloadCompletedEvent fileDownloadCompletedEvent;

        public delegate void CancelDownLoadEvent(CancelReason reason);
        public event CancelDownLoadEvent cancelDownloadEvent;

        public FileDownloader(Queue<LocalItemDetails> queue, ThreadLockObject lockObject, CloudService fileCloud, bool analysisCompleted)
        {
            LogWrapper.LogMessage("FileDownloader - Constructor", "Enter");
            LogWrapper.LogMessage("FileDownloader - Constructor", "Setting queue with count: " + queue.Count);
            this.queue = queue;
            this.lockObject = lockObject;
            cFileCloud = fileCloud;
            IsAnalysisCompleted = analysisCompleted;
            LogWrapper.LogMessage("FileDownloader - Constructor", "Opening DB connection");
            dbhandler.OpenConnection();
            LogWrapper.LogMessage("FileDownloader - Constructor", "Leave");
            
        }

        public void consume()
        {
            LogWrapper.LogMessage("FileDownloader - consume", "Enter");
            LocalItemDetails itemDetail;
            bool done = false;

            while (!done)
            {
                lock (lockObject)
                {
                    LogWrapper.LogMessage("FileDownloader - consume", "lockObject - locked");
                    if (lockObject.StopThread)
                    {
                        LogWrapper.LogMessage("FileDownloader - consume", "Stop requested");
                        done = true;
                        LogWrapper.LogMessage("FileDownloader - consume", "calling CancelAndNotify with reason USER_CANCEL");
                        CancelAndNotify(CancelReason.USER_CANCEL);
                        break;
                    }

                    if (queue.Count == 0)
                    {
                        if (!IsAnalysisCompleted)
                        {
                            LogWrapper.LogMessage("FileDownloader - consume", "queue count ZERO, waiting on lockObject");
                            Monitor.Wait(lockObject);
                            continue;
                        }
                        else
                        {
                            if (fileDownloadCompletedEvent != null)
                            {
                                LogWrapper.LogMessage("FileDownloader - consume", "calling fileDownloadCompletedEvent");
                                done = true;
                                fileDownloadCompletedEvent();
                                break;
                            }
                        }

                    }

                    LogWrapper.LogMessage("FileDownloader - consume", "dequeue item from queue");
                    itemDetail = queue.Dequeue();
                    LogWrapper.LogMessage("FileDownloader - consume", "lockObject - unlocked");
                }

                if (itemDetail == null || itemDetail.ItemDetails == null)
                {
                    LogWrapper.LogMessage("FileDownloader - consume", "itemDetail null, continue loop");
                    continue;
                }

                ItemDetails id = itemDetail.ItemDetails;
                //foreach (ItemDetails id in itemDetail.ItemDetails)
                {
                    if (lockObject.StopThread)
                    {
                        LogWrapper.LogMessage("FileDownloader - consume", "Stop requested");
                        done = true;
                        LogWrapper.LogMessage("FileDownloader - consume", "calling CancelAndNotify with reason USER_CANCEL");
                        CancelAndNotify(CancelReason.USER_CANCEL);
                        break;
                    }

                    LogWrapper.LogMessage("FileDownloader - consume", "creating file folder info for " + id.strName);

                    FileFolderInfo fileFolderInfo = new FileFolderInfo();

                        
                    using (fileFolderInfo)
                    {
                        fileFolderInfo.Key = itemDetail.Path;

                        fileFolderInfo.IsPublic = id.bPublic;//id.strPublic.Trim().Length == 0 ? false : Convert.ToBoolean(id.strPublic);
                        fileFolderInfo.IsShared = id.bShared;//Convert.ToBoolean(id.strShared);
                        fileFolderInfo.ContentUrl = id.szContentUrl;
                        fileFolderInfo.CreatedDate = id.dtCreated;//DateTime.Parse(id.strCreated);
                        fileFolderInfo.FileName = id.strName;
                        fileFolderInfo.FileSize = id.dblSizeInBytes;
                        fileFolderInfo.MimeType = id.szMimeType;
                        fileFolderInfo.ModifiedDate = id.dtModified;//DateTime.Parse(id.strModified);
                        fileFolderInfo.ParentUrl = id.szParentUrl;
                        fileFolderInfo.Status = "INPROGRESS";
                        fileFolderInfo.Type = id.szItemType;

                        int lastSepIndex = itemDetail.Path.LastIndexOf("\\");
                        string parentDirPath = "";

                        if (lastSepIndex != -1)
                        {                                
                            parentDirPath = itemDetail.Path.Substring(0, itemDetail.Path.LastIndexOf("\\"));
                            parentDirPath = parentDirPath.Substring(parentDirPath.LastIndexOf("\\") + 1);
                        }
                        //else
                        //{
                        //    parentDirPath = itemDetail.Path;
                        //}

                        fileFolderInfo.ParentDir = parentDirPath;

                        if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
                        if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

                        LogWrapper.LogMessage("FileDownloader - consume", "writing file folder info for " + id.strName + " in DB");
                        dbhandler.Write(fileFolderInfo);

                        string downloadObjectName = BasicInfo.SyncDirPath + "\\";
                        downloadObjectName += itemDetail.Path;

                        LogWrapper.LogMessage("FileDownloader - consume", "download object " + downloadObjectName);

                        LogWrapper.LogMessage("FileDownloader - consume", "setting parent folders status DB_STATUS_IN_PROGRESS, bRet FALSE");
                        MarkParentsStatus(downloadObjectName, DB_STATUS_IN_PROGRESS);
                        bool bRet = false;
                        int refCode = 0;

                        if (id.szItemType == "DIRECTORY")
                        {
                            LogWrapper.LogMessage("FileDownloader - consume", id.strName + " is DIRECTORY");
                            System.IO.Directory.CreateDirectory(downloadObjectName);

                            if (id.strETag.Trim().Length == 0)
                            {
                                LogWrapper.LogMessage("FileDownloader - consume", "Getting eTag for " + id.strName);
                                id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                                if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
                                {
                                    lockObject.StopThread = true;
                                    done = true;
                                    CancelAndNotify(CancelReason.LOGIN_FAILED);
                                    break;
                                }
                                else if (refCode != ResponseCode.GETETAG)
                                {
                                    lockObject.StopThread = true;
                                    done = true;
                                    CancelAndNotify(CancelReason.SERVER_INACCESSIBLE);
                                    break;
                                }
                            }

                            LogWrapper.LogMessage("FileDownloader - consume", "eTag for " + id.strName + ": " + id.strETag + ", bRet TRUE");
                            bRet = true;
                        }
                        else
                        {
                            LogWrapper.LogMessage("FileDownloader - consume", id.strName + " is NOT DIRECTORY");
                            bRet = cFileCloud.DownloadFile(id.szContentUrl + '/' + id.strName,
                                                    downloadObjectName, id.dblSizeInBytes, ref refCode);
                           
                            if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
                            {
                                lockObject.StopThread = true;
                                done = true;
                                CancelAndNotify(CancelReason.LOGIN_FAILED);
                                break;
                            }
                            else if (refCode != ResponseCode.DOWNLOADFILE)
                            {
                                lockObject.StopThread = true;
                                done = true;
                                CancelAndNotify(CancelReason.SERVER_INACCESSIBLE);
                                break;
                            }

                            LogWrapper.LogMessage("FileDownloader - consume", "bRet for " + id.strName + " is " + bRet.ToString());
                            if (refCode == INSUFFICIENT_STORAGE_AVAILABLE)
                            {
                                LogWrapper.LogMessage("FileDownloader - consume", "INSUFFICIENT_STORAGE_AVAILABLE, calling CancelAndNotify with reason INSUFFICIENT_STORAGE");
                                done = true;
                                CancelAndNotify(CancelReason.INSUFFICIENT_STORAGE);
                                break;
                            }

                            LogWrapper.LogMessage("FileDownloader - consume", "Getting eTag for " + id.strName);
                            id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                            
                            if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
                            {
                                lockObject.StopThread = true;
                                done = true;
                                CancelAndNotify(CancelReason.LOGIN_FAILED);
                                break;
                            }
                            else if (refCode != ResponseCode.GETETAG)
                            {
                                lockObject.StopThread = true;
                                done = true;
                                CancelAndNotify(CancelReason.SERVER_INACCESSIBLE);
                                break;
                            }
                            LogWrapper.LogMessage("FileDownloader - consume", "eTag for " + id.strName + ": " + id.strETag);
                        }

                        if (!bRet)
                        {
                            LogWrapper.LogMessage("FileDownloader - consume", "bRet FALSE, writing to cFileCloud.AppEventViewer");
                            string Description = "";//AboutBox.AssemblyTitle;

                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload1");
                           // Description += AboutBox.AssemblyProduct;
                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload2");
                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload3");
                           // cFileCloud.AppEventViewer(AboutBox.AssemblyTitle, Description, 3);
                        }
                        else
                        {
                            LogWrapper.LogMessage("FileDownloader - consume", "setting parent folders status to DB_STATUS_SUCCESS for " + downloadObjectName);
                            MarkParentsStatus(downloadObjectName, DB_STATUS_SUCCESS);
                            //fileFolderInfo.ETag = id.strETag;
                            if (id.szItemType == "DIRECTORY")
                            {
                                LogWrapper.LogMessage("FileDownloader - consume", "updating DB for folder " + downloadObjectName);
                                DirectoryInfo dInfo = new DirectoryInfo(downloadObjectName);
                                dbhandler.UpdateModifiedDate(dInfo.LastWriteTime, fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , id.strETag , DbHandler.KEY , fileFolderInfo.Key );
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS ,"SUCCESS", DbHandler.KEY , fileFolderInfo.Key );
                            }
                            else
                            {
                                LogWrapper.LogMessage("FileDownloader - consume", "updating DB for file " + downloadObjectName);
                                FileInfo fInfo = new FileInfo(downloadObjectName);
                                dbhandler.UpdateModifiedDate(fInfo.LastWriteTime, fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , id.strETag , DbHandler.KEY , fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS,"SUCCESS", DbHandler.KEY , fileFolderInfo.Key );
                            }

                            if (downloadEvent != null)
                            {
                                LogWrapper.LogMessage("FileDownloader - consume", "calling  downloadEvent with " + downloadObjectName);
                                downloadEvent(this, new FileDownloaderEvents(downloadObjectName, 0));
                            }
                        }
                       
                    }
                        
                }

                if (IsAnalysisCompleted && queue.Count == 0)
                {
                    LogWrapper.LogMessage("FileDownloader - consume", "Analysis completed and queue lenth is ZERO");
                    if (fileDownloadCompletedEvent != null)
                    {
                        LogWrapper.LogMessage("FileDownloader - consume", "calling fileDownloadCompletedEvent");
                        done = true;
                        fileDownloadCompletedEvent();
                    }
                }
                
            }

            LogWrapper.LogMessage("FileDownloader - consume", "Leave");
        }

        private void MarkParentsStatus(string path, string status)
        {
            LogWrapper.LogMessage("FileDownloader - MarkParentsStatus", "MarkParentsStatus for path " + path + " with status " + status);
            string syncPath = path.Substring(BasicInfo.SyncDirPath.Length + 1);
            ChangeParentStatus(syncPath, status);
        }

        private void ChangeParentStatus(string syncPath, string status)
        {
            LogWrapper.LogMessage("FileDownloader - ChangeParentStatus", "ChangeParentStatus for path " + syncPath + " with status " + status);
            int sepIndex = syncPath.LastIndexOf("\\");

            if (sepIndex > 0)
            {
                string parentKey = syncPath.Substring(0, sepIndex);
                LogWrapper.LogMessage("FileDownloader - ChangeParentStatus", "updating DB for key " + parentKey + " with status " + status);
                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , status , DbHandler.KEY , parentKey );
                LogWrapper.LogMessage("FileDownloader - ChangeParentStatus", "DB update DONE");
                ChangeParentStatus(parentKey, status);
            }
        }

        public void ForceComplete()
        {
            LogWrapper.LogMessage("FileDownloader - ForceComplete", "Called ForceComplete");
            if (fileDownloadCompletedEvent != null)
            {
                LogWrapper.LogMessage("FileDownloader - ForceComplete", "calling fileDownloadCompletedEvent");
                fileDownloadCompletedEvent();
            }
        }

        private void CancelAndNotify(CancelReason reason)
        {
            LogWrapper.LogMessage("FileDownloader - CancelAndNotify", "called CancelAndNotify");
            if (cancelDownloadEvent != null)
            {
                LogWrapper.LogMessage("FileDownloader - CancelAndNotify", "calling CancelAndNotify");
                cancelDownloadEvent(reason);
            }
        }

    }
}
