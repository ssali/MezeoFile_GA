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

        Queue<LocalItemDetails> queue;
        ThreadLockObject lockObject;
        MezeoFileCloud cFileCloud;
        DbHandler dbhandler = new DbHandler();
        public bool IsAnalysisCompleted { get; set; }

        public delegate void FileDownloadEvent(object sender, FileDownloaderEvents e);
        public event FileDownloadEvent downloadEvent;

        public delegate void FileDownloadCompletedEvent();
        public event FileDownloadCompletedEvent fileDownloadCompletedEvent;

        public delegate void CancelDownLoadEvent();
        public event CancelDownLoadEvent cancelDownloadEvent;

        public FileDownloader(Queue<LocalItemDetails> queue, ThreadLockObject lockObject, MezeoFileCloud fileCloud, bool analysisCompleted)
        {
            this.queue = queue;
            this.lockObject = lockObject;
            cFileCloud = fileCloud;
            IsAnalysisCompleted = analysisCompleted;
            dbhandler.OpenConnection();
        }

        public void consume()
        {
            LocalItemDetails itemDetail;
            bool done = false;

            while (!done)
            {
                lock (lockObject)
                {
                    if (lockObject.StopThread)
                    {
                        done = true;
                        CancelAndNotify();
                        break;
                    }

                    if (queue.Count == 0)
                    {
                        Monitor.Wait(lockObject);
                        continue;

                    }

                    itemDetail = queue.Dequeue();
                }

                if (itemDetail == null || itemDetail.ItemDetails == null)
                    continue;

                ItemDetails id = itemDetail.ItemDetails;
                //foreach (ItemDetails id in itemDetail.ItemDetails)
                {
                    if (lockObject.StopThread)
                    {
                        done = true;
                        CancelAndNotify();
                        break;
                    }

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

                        dbhandler.Write(fileFolderInfo);

                        string downloadObjectName = BasicInfo.SyncDirPath + "\\";
                        downloadObjectName += itemDetail.Path;

                        MarkParentsStatus(downloadObjectName, DB_STATUS_IN_PROGRESS);
                        bool bRet = false;
                        int refCode = 0;

                        if (id.szItemType == "DIRECTORY")
                        {
                            System.IO.Directory.CreateDirectory(downloadObjectName);
                            
                            if (id.strETag.Trim().Length == 0)
                                id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                            
                            bRet = true;
                        }
                        else
                        {        
                            bRet = cFileCloud.DownloadFile(id.szContentUrl + '/' + id.strName,
                                                    downloadObjectName, ref refCode);

                            if (refCode != 200)
                            {
                                done = true;
                                CancelAndNotify();
                                break;
                            }

                            id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                        }

                        if (!bRet)
                        {
                            string Description = AboutBox.AssemblyTitle;

                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload1");
                            Description += AboutBox.AssemblyProduct;
                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload2");
                            Description += LanguageTranslator.GetValue("ErrorBlurbDownload3");
                            cFileCloud.AppEventViewer(AboutBox.AssemblyTitle, Description, 3);
                        }
                        else
                        {
                            MarkParentsStatus(downloadObjectName, DB_STATUS_SUCCESS);
                            //fileFolderInfo.ETag = id.strETag;
                            if (id.szItemType == "DIRECTORY")
                            {
                                DirectoryInfo dInfo = new DirectoryInfo(downloadObjectName);
                                dbhandler.UpdateModifiedDate(dInfo.LastWriteTime, fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + "='" + id.strETag + "'", DbHandler.KEY + "='" + fileFolderInfo.Key + "'");
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS + "= 'SUCCESS'", DbHandler.KEY + "='" + fileFolderInfo.Key + "'");
                            }
                            else
                            {
                                FileInfo fInfo = new FileInfo(downloadObjectName);
                                dbhandler.UpdateModifiedDate(fInfo.LastWriteTime, fileFolderInfo.Key);
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.E_TAG + "='" + id.strETag + "'", DbHandler.KEY + "='" + fileFolderInfo.Key + "'");
                                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS + "= 'SUCCESS'", DbHandler.KEY + "='" + fileFolderInfo.Key + "'");
                            }

                            if (downloadEvent != null)
                            {
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
                    if (fileDownloadCompletedEvent != null)
                    {
                        done = true;
                        fileDownloadCompletedEvent();
                    }
                }
                
            }
        }

        private void MarkParentsStatus(string path, string status)
        {
            string syncPath = path.Substring(BasicInfo.SyncDirPath.Length + 1);
            ChangeParentStatus(syncPath, status);
        }

        private void ChangeParentStatus(string syncPath, string status)
        {
            int sepIndex = syncPath.LastIndexOf("\\");

            if (sepIndex > 0)
            {
                string parentKey = syncPath.Substring(0, sepIndex);
                dbhandler.Update(DbHandler.TABLE_NAME, DbHandler.STATUS + "='" + status + "'", DbHandler.KEY + "='" + parentKey + "'");
                ChangeParentStatus(parentKey, status);
            }
        }

        public void ForceComplete()
        {
            if (fileDownloadCompletedEvent != null)
            {
                fileDownloadCompletedEvent();
            }
        }

        private void CancelAndNotify()
        {
            if (cancelDownloadEvent != null)
            {
                cancelDownloadEvent();
            }
        }

    }
}
