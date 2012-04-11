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
        private static int FILE_DOWNLOAD_SUCCESS = 200;
        Debugger debugger = new Debugger();

        public enum CancelReason
        {
            INSUFFICIENT_STORAGE,
            USER_CANCEL,
            DOWNLOAD_FAILED
        }

        Queue<LocalItemDetails> queue;
        ThreadLockObject lockObject;
        MezeoFileCloud cFileCloud;
        DbHandler dbhandler = new DbHandler();
        public bool IsAnalysisCompleted { get; set; }

        public delegate void FileDownloadEvent(object sender, FileDownloaderEvents e);
        public event FileDownloadEvent downloadEvent;

        public delegate void FileDownloadCompletedEvent();
        public event FileDownloadCompletedEvent fileDownloadCompletedEvent;

        public delegate void CancelDownLoadEvent(CancelReason reason);
        public event CancelDownLoadEvent cancelDownloadEvent;

        public FileDownloader(Queue<LocalItemDetails> queue, ThreadLockObject lockObject, MezeoFileCloud fileCloud, bool analysisCompleted)
        {
            debugger.logMessage("FileDownloader - Constructor", "Enter");
            debugger.logMessage("FileDownloader - Constructor", "Setting queue with count: " + queue.Count);
            this.queue = queue;
            this.lockObject = lockObject;
            cFileCloud = fileCloud;
            IsAnalysisCompleted = analysisCompleted;
            debugger.logMessage("FileDownloader - Constructor", "Opening DB connection");
            dbhandler.OpenConnection();
            debugger.logMessage("FileDownloader - Constructor", "Leave");
        }

        public void consume()
        {
            debugger.logMessage("FileDownloader - consume", "Enter");
            LocalItemDetails itemDetail;
            bool done = false;

            while (!done)
            {
                lock (lockObject)
                {
                    debugger.logMessage("FileDownloader - consume", "lockObject - locked");
                    if (lockObject.StopThread)
                    {
                        debugger.logMessage("FileDownloader - consume", "Stop requested");
                        done = true;
                        debugger.logMessage("FileDownloader - consume", "calling CancelAndNotify with reason USER_CANCEL");
                        CancelAndNotify(CancelReason.USER_CANCEL);
                        break;
                    }

                    if (queue.Count == 0)
                    {
                        debugger.logMessage("FileDownloader - consume", "queue count ZERO, waiting on lockObject");
                        Monitor.Wait(lockObject);
                        continue;

                    }

                    debugger.logMessage("FileDownloader - consume", "dequeue item from queue");
                    itemDetail = queue.Dequeue();
                    debugger.logMessage("FileDownloader - consume", "lockObject - unlocked");
                }

                if (itemDetail == null || itemDetail.ItemDetails == null)
                {
                    debugger.logMessage("FileDownloader - consume", "itemDetail null, continue loop");
                    continue;
                }

                ItemDetails id = itemDetail.ItemDetails;
                //foreach (ItemDetails id in itemDetail.ItemDetails)
                {
                    if (lockObject.StopThread)
                    {
                        debugger.logMessage("FileDownloader - consume", "Stop requested");
                        done = true;
                        debugger.logMessage("FileDownloader - consume", "calling CancelAndNotify with reason USER_CANCEL");
                        CancelAndNotify(CancelReason.USER_CANCEL);
                        break;
                    }

                    debugger.logMessage("FileDownloader - consume", "creating file folder info for " + id.strName);

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

                        debugger.logMessage("FileDownloader - consume", "writing file folder info for " + id.strName + " in DB");
                        dbhandler.Write(fileFolderInfo);

                        string downloadObjectName = BasicInfo.SyncDirPath + "\\";
                        downloadObjectName += itemDetail.Path;

                        debugger.logMessage("FileDownloader - consume", "download object " + downloadObjectName);

                        debugger.logMessage("FileDownloader - consume", "setting parent folders status DB_STATUS_IN_PROGRESS, bRet FALSE");
                        MarkParentsStatus(downloadObjectName, DB_STATUS_IN_PROGRESS);
                        bool bRet = false;
                        int refCode = 0;

                        if (id.szItemType == "DIRECTORY")
                        {
                            debugger.logMessage("FileDownloader - consume", id.strName + " is DIRECTORY");
                            System.IO.Directory.CreateDirectory(downloadObjectName);

                            if (id.strETag.Trim().Length == 0)
                            {
                                debugger.logMessage("FileDownloader - consume", "Getting eTag for " + id.strName );
                                id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                            }
                            debugger.logMessage("FileDownloader - consume", "eTag for " + id.strName + ": " + id.strETag + ", bRet TRUE");
                            bRet = true;
                        }
                        else
                        {
                            debugger.logMessage("FileDownloader - consume", id.strName + " is NOT DIRECTORY");
                            bRet = cFileCloud.DownloadFile(id.szContentUrl + '/' + id.strName,
                                                    downloadObjectName, id.dblSizeInBytes, ref refCode);

                            debugger.logMessage("FileDownloader - consume", "bRet for " + id.strName + " is " + bRet.ToString());
                            if (refCode == INSUFFICIENT_STORAGE_AVAILABLE)
                            {
                                debugger.logMessage("FileDownloader - consume", "INSUFFICIENT_STORAGE_AVAILABLE, calling CancelAndNotify with reason INSUFFICIENT_STORAGE");
                                done = true;
                                CancelAndNotify(CancelReason.INSUFFICIENT_STORAGE);
                                break;
                            }
                            //else if (refCode != FILE_DOWNLOAD_SUCCESS)
                            //{
                            //    done = true;
                            //    CancelAndNotify(CancelReason.DOWNLOAD_FAILED);
                            //    break;
                            //}
                            debugger.logMessage("FileDownloader - consume", "Getting eTag for " + id.strName);
                            id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                            debugger.logMessage("FileDownloader - consume", "eTag for " + id.strName + ": " + id.strETag );
                        }

                        if (!bRet)
                        {
                            debugger.logMessage("FileDownloader - consume", "bRet FALSE, writing to cFileCloud.AppEventViewer");
                            string Description = AboutBox.AssemblyTitle;

                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload1");
                            Description += AboutBox.AssemblyProduct;
                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload2");
                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload3");
                            cFileCloud.AppEventViewer(AboutBox.AssemblyTitle, Description, 3);
                        }
                        else
                        {
                            debugger.logMessage("FileDownloader - consume", "setting parent folders status to DB_STATUS_SUCCESS for " + downloadObjectName);
                            MarkParentsStatus(downloadObjectName, DB_STATUS_SUCCESS);
                            //fileFolderInfo.ETag = id.strETag;
                            if (id.szItemType == "DIRECTORY")
                            {
                                debugger.logMessage("FileDownloader - consume", "updating DB for folder " + downloadObjectName);
                                DirectoryInfo dInfo = new DirectoryInfo(downloadObjectName);
                                dbhandler.UpdateModifiedDate(dInfo.LastWriteTime, fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , id.strETag , DbHandler.KEY , fileFolderInfo.Key );
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS ,"SUCCESS", DbHandler.KEY , fileFolderInfo.Key );
                            }
                            else
                            {
                                debugger.logMessage("FileDownloader - consume", "updating DB for file " + downloadObjectName);
                                FileInfo fInfo = new FileInfo(downloadObjectName);
                                dbhandler.UpdateModifiedDate(fInfo.LastWriteTime, fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG , id.strETag , DbHandler.KEY , fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS,"SUCCESS", DbHandler.KEY , fileFolderInfo.Key );
                            }

                            if (downloadEvent != null)
                            {
                                debugger.logMessage("FileDownloader - consume", "calling  downloadEvent with " + downloadObjectName);
                                downloadEvent(this, new FileDownloaderEvents(downloadObjectName, 0));
                            }
                        }
                       
                        //if (downloadEvent != null)
                        //{
                        //    downloadEvent(this, new FileDownloaderEvents(downloadObjectName, 0));
                        //}

                    }
                        
                }

                if (IsAnalysisCompleted && queue.Count == 0)
                {
                    debugger.logMessage("FileDownloader - consume", "Analysis completed and queue lenth is ZERO");
                    if (fileDownloadCompletedEvent != null)
                    {
                        debugger.logMessage("FileDownloader - consume", "calling fileDownloadCompletedEvent");
                        done = true;
                        fileDownloadCompletedEvent();
                    }
                }
                
            }

            debugger.logMessage("FileDownloader - consume", "Leave");
        }

        private void MarkParentsStatus(string path, string status)
        {
            debugger.logMessage("FileDownloader - MarkParentsStatus", "MarkParentsStatus for path " + path + " with status " + status);
            string syncPath = path.Substring(BasicInfo.SyncDirPath.Length + 1);
            ChangeParentStatus(syncPath, status);
        }

        private void ChangeParentStatus(string syncPath, string status)
        {
            debugger.logMessage("FileDownloader - ChangeParentStatus", "ChangeParentStatus for path " + syncPath + " with status " + status);
            int sepIndex = syncPath.LastIndexOf("\\");

            if (sepIndex > 0)
            {
                string parentKey = syncPath.Substring(0, sepIndex);
                debugger.logMessage("FileDownloader - ChangeParentStatus", "updating DB for key " + parentKey + " with status " + status);
                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS , status , DbHandler.KEY , parentKey );
                debugger.logMessage("FileDownloader - ChangeParentStatus", "DB update DONE");
                ChangeParentStatus(parentKey, status);
            }
        }

        public void ForceComplete()
        {
            debugger.logMessage("FileDownloader - ForceComplete", "Called ForceComplete");
            if (fileDownloadCompletedEvent != null)
            {
                debugger.logMessage("FileDownloader - ForceComplete", "calling fileDownloadCompletedEvent");
                fileDownloadCompletedEvent();
            }
        }

        private void CancelAndNotify(CancelReason reason)
        {
            debugger.logMessage("FileDownloader - CancelAndNotify", "called CancelAndNotify");
            if (cancelDownloadEvent != null)
            {
                debugger.logMessage("FileDownloader - CancelAndNotify", "calling CancelAndNotify");
                cancelDownloadEvent(reason);
            }
        }

    }
}
