using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MezeoFileSupport;

namespace Mezeo
{
    class StructureDownloader
    {
        public delegate void StructureDownloadEvent(object sender, StructureDownloaderEvent e);
        public event StructureDownloadEvent downloadEvent;

        public delegate void CancelDownLoadEvent(CancelReason reason);
        public event CancelDownLoadEvent cancelDownloadEvent;

        public delegate void StartDownLoaderEvent(bool bStart);
        public event StartDownLoaderEvent startDownloaderEvent;

        private int totalFileCount = 0;

        DbHandler dbhandler = new DbHandler();

        public int TotalFileCount
        {
            get
            {
                return totalFileCount;
            }
        }

        Queue<LocalItemDetails> queue;
        ThreadLockObject lockObject;
        string cRootContainerUrl;
        CloudService cFileCloud;

        bool isRootContainer = false;

        public StructureDownloader(Queue<LocalItemDetails> queue, ThreadLockObject lockObject, string rootContainerUrl, CloudService fileCloud)
        {
            LogWrapper.LogMessage("StructureDownloader - Constructor", "Enter");
            this.queue = queue;
            this.lockObject = lockObject;
            cRootContainerUrl = rootContainerUrl;
            cFileCloud = fileCloud;
            dbhandler.OpenConnection();

            LogWrapper.LogMessage("StructureDownloader - Constructor Call", "Content Url: " + rootContainerUrl);
            LogWrapper.LogMessage("StructureDownloader - Constructor", "Leave");
        }

        public void PrepareStructure(LocalItemDetails lItemdDetails)
        {
            LogWrapper.LogMessage("StructureDownloader - PrepareStructure", "Enter");

                lock (lockObject)
                {
                    queue.Enqueue(lItemdDetails);
                    if (isRootContainer)
                    {
                        LogWrapper.LogMessage("StructureDownloader - PrepareStructure", "Pulse");
                        Monitor.PulseAll(lockObject);
                    }
                }

                LogWrapper.LogMessage("StructureDownloader - PrepareStructure", "Leave");
        }

        public void startAnalyseItemDetails()
        {
            LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Enter");

            int refCode=0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(cRootContainerUrl, ref refCode);
            
            if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
            {
                lockObject.StopThread = true;
                CancelAndNotify(CancelReason.LOGIN_FAILED);
                return;
            }
            else if (refCode == -1)
            {
                // The socket timed out or something else occured.  Let the normal timer
                // handle the online/offline mode stuff and just return as if normal
                // processing occured.  Otherwise, the sync will never progress past
                // this point.
                return;
            }
            else if (refCode != ResponseCode.DOWNLOADITEMDETAILS)
            {
                lockObject.StopThread = true;
                CancelAndNotify(CancelReason.SERVER_INACCESSIBLE);
                return;
            }

            if (contents == null)
            {
                LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Contents Null");

                if (downloadEvent != null)
                {

                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Called download event with TRUE");
                    downloadEvent(this, new StructureDownloaderEvent(true));
                }

                if (startDownloaderEvent != null)
                {
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Called startDownloaderEvent event with FALSE");
                    startDownloaderEvent(false);
                }
                LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Leaving as contents are null");
                return;
            }

            if (startDownloaderEvent != null)
            {
                LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Called startDownloaderEvent event with TRUE");
                startDownloaderEvent(true);
            }

            isRootContainer = true;

            LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "setting isRootContainer to TRUE");
            LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Contents Length: " + contents.Length);
            foreach (ItemDetails iDetail in contents)
            {
                LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Checking KEY in DB for content url: " + iDetail.szContentUrl + " with SUCCESS");

                string strCheck = dbhandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL, DbHandler.STATUS }, new string[] { iDetail.szContentUrl, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                if (strCheck.Trim().Length == 0)
                {
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "KEY for content url: " + iDetail.szContentUrl + " with SUCCESS not found in DB");
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Creating a new LocalItemDetails with Path: " + iDetail.strName);
                    LocalItemDetails lItem = new LocalItemDetails();
                    lItem.ItemDetails = iDetail;
                    lItem.Path = iDetail.strName;
                    totalFileCount++;
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "totalFileCount: " + totalFileCount + "\n Calling PrepareStructure with lItem");
                    PrepareStructure(lItem);
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "returned from PrepareStructure");
                }
            }

            LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Contents total item count: " + contents[0].nTotalItem);
            for (int n = 0; n < contents[0].nTotalItem; n++)
            {
                if (lockObject.StopThread /*|| refCode != 200*/)
                {
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Stop thread requested Calling CancelAndNotify");
                    CancelAndNotify(CancelReason.USER_CANCEL);
                    break;
                }
                if (contents[n].szItemType == "DIRECTORY")
                {
                    LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Calling analyseItemDetails for DIR: " + contents[n].strName);
                    analyseItemDetails(contents[n], contents[n].strName);
                }
            }

            if (downloadEvent != null)
            {
                LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Calling downloadEvent with TRUE");
                downloadEvent(this, new StructureDownloaderEvent(true));
            }

            LogWrapper.LogMessage("StructureDownloader - startAnalyseItemDetails", "Leave");
        }

        public void analyseItemDetails(ItemDetails itemDetail,string strPath)
        {
            LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Enter");
            int refCode = 0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(itemDetail.szContentUrl, ref refCode);
            
            if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
            {
                lockObject.StopThread = true;
                CancelAndNotify(CancelReason.LOGIN_FAILED);
                return;
            }
            else if (refCode == -1)
            {
                // The socket timed out or something else occured.  Let the normal timer
                // handle the online/offline mode stuff and just return as if normal
                // processing occured.  Otherwise, the sync will never progress past
                // this point.
                return;
            }
            else if (refCode != ResponseCode.DOWNLOADITEMDETAILS)
            {
                lockObject.StopThread = true;
                CancelAndNotify(CancelReason.SERVER_INACCESSIBLE);
                return;
            }

            if (contents == null)
            {
                LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Contents Null");
                return;
            }

            foreach (ItemDetails iDetail in contents)
            {
                LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Checking KEY in DB for content url: " + iDetail.szContentUrl + " with SUCCESS");

                string strCheck = dbhandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL, DbHandler.STATUS }, new string[] { iDetail.szContentUrl, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                if (strCheck.Trim().Length == 0)
                {
                    LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "KEY for content url: " + iDetail.szContentUrl + " with SUCCESS not found in DB");
                    LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Creating a new LocalItemDetails with Path: " + iDetail.strName);
                    LocalItemDetails lItem = new LocalItemDetails();
                    lItem.ItemDetails = iDetail;
                    lItem.Path += strPath;
                    lItem.Path += "\\" + iDetail.strName;
                    totalFileCount++;
                    LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "totalFileCount: " + totalFileCount + "\n Calling PrepareStructure with lItem");
                    PrepareStructure(lItem);
                    LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "returned from PrepareStructure");
                }
            }

            LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Contents total item count: " + contents[0].nTotalItem);
            if (contents[0].nTotalItem > 0)
            {
                for (int n = 0; n < contents[0].nTotalItem; n++)
                {
                    if (lockObject.StopThread /*|| refCode != 200*/)
                    {
                        LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Stop thread requested Calling CancelAndNotify");
                        CancelAndNotify(CancelReason.USER_CANCEL);
                        break;
                    }

                    if (contents[n].szItemType == "DIRECTORY")
                    {
                        LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Calling analyseItemDetails for DIR: " + contents[n].strName);
                        analyseItemDetails(contents[n], strPath + "\\" + contents[n].strName);
                    }
                }
            }

            LogWrapper.LogMessage("StructureDownloader - analyseItemDetails", "Leave");
        }

        private void CancelAndNotify(CancelReason reason)
        {
            LogWrapper.LogMessage("StructureDownloader - CancelAndNotify", "Enter");
            if(cancelDownloadEvent != null)
            {
                cancelDownloadEvent(reason);
            }
            LogWrapper.LogMessage("StructureDownloader - CancelAndNotify", "Leave");
        }
    }
}
