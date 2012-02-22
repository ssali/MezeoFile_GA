using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MezeoFileSupport;
using System.Text.RegularExpressions;

namespace Mezeo
{
    class FileDownloader
    {
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
                    foreach (ItemDetails id in itemDetail.ItemDetails)
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
                            if (itemDetail.Path.Trim().Length == 0)
                            {
                                fileFolderInfo.Key =  id.strName;
                            }
                            else
                            {
                                fileFolderInfo.Key = itemDetail.Path + "\\" + id.strName;
                            }
                            
                            fileFolderInfo.IsPublic = Convert.ToBoolean(id.strPublic);
                            fileFolderInfo.IsShared = Convert.ToBoolean(id.strShared);
                            fileFolderInfo.ContentUrl = id.szContentUrl;
                            fileFolderInfo.CreatedDate = DateTime.Parse(id.strCreated);
                            fileFolderInfo.FileName = id.strName;
                            fileFolderInfo.FileSize = id.dblSizeInBytes;
                            fileFolderInfo.MimeType = id.szMimeType;
                            fileFolderInfo.ModifiedDate = DateTime.Parse(id.strModified);
                            fileFolderInfo.ParentUrl = id.szParentUrl;
                            fileFolderInfo.Status = "";
                            fileFolderInfo.Type = id.szItemType;

                            int lastSepIndex = itemDetail.Path.LastIndexOf("\\");
                            string parentDirPath = "";

                            if (lastSepIndex > 0)
                            {
                                parentDirPath = itemDetail.Path.Substring(lastSepIndex + 1);
                            }

                            fileFolderInfo.ParentDir = parentDirPath;

                            if (id.szItemType == "DIRECTORY")
                            {
                                if (itemDetail.Path.Length != 0)
                                    System.IO.Directory.CreateDirectory(BasicInfo.SyncDirPath + "\\" + itemDetail.Path + "\\" + id.strName);
                                else
                                    System.IO.Directory.CreateDirectory(BasicInfo.SyncDirPath + "\\" + id.strName);
                            }
                            else
                            {

                                string downloadFileName = BasicInfo.SyncDirPath + "\\" + itemDetail.Path + "\\" + id.strName;
                                if (downloadEvent != null)
                                {
                                    downloadEvent(this, new FileDownloaderEvents(downloadFileName, 0));
                                }

                                int refCode = 0;
                                cFileCloud.DownloadFile(id.szContentUrl + '/' + id.strName,
                                                        downloadFileName, ref refCode);

                                id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                            }

                            fileFolderInfo.ETag = id.strETag;

                            if (fileFolderInfo.ETag == null) { fileFolderInfo.ETag = ""; }
                            if (fileFolderInfo.MimeType == null) { fileFolderInfo.MimeType = ""; }

                            dbhandler.Write(fileFolderInfo);
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
