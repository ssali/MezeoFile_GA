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
        CloudService cFileCloud;//=new MezeoFileCloud();

        bool isRootContainer = false;

        //static int seq = 0;
        public StructureDownloader(Queue<LocalItemDetails> queue, ThreadLockObject lockObject, string rootContainerUrl, CloudService fileCloud)
        {
            Debugger.Instance.logMessage("StructureDownloader - Constructor", "Enter");
            this.queue = queue;
            this.lockObject = lockObject;
            cRootContainerUrl = rootContainerUrl;
            cFileCloud = fileCloud;
            dbhandler.OpenConnection();

            Debugger.Instance.logMessage("StructureDownloader - Constructor Call", "Content Url: " + rootContainerUrl);
            Debugger.Instance.logMessage("StructureDownloader - Constructor", "Leave");
           // Debugger.Instance.Instance.ShowLogger();
        }

        public void PrepareStructure(LocalItemDetails lItemdDetails)
        {
            Debugger.Instance.logMessage("StructureDownloader - PrepareStructure", "Enter");

                lock (lockObject)
                {
                    queue.Enqueue(lItemdDetails);
                    if (isRootContainer)
                    {
                        Debugger.Instance.logMessage("StructureDownloader - PrepareStructure", "Pulse");
                        Monitor.PulseAll(lockObject);
                    }
                }

                Debugger.Instance.logMessage("StructureDownloader - PrepareStructure", "Leave");
        }

        public void startAnalyseItemDetails()
        {
            Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Enter");

            int refCode=0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(cRootContainerUrl, ref refCode);
            
            if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
            {
                lockObject.StopThread = true;
                CancelAndNotify(CancelReason.LOGIN_FAILED);
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
                Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Contents Null");

                if (downloadEvent != null)
                {

                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Called download event with TRUE");
                    downloadEvent(this, new StructureDownloaderEvent(true));
                }

                if (startDownloaderEvent != null)
                {
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Called startDownloaderEvent event with FALSE");
                    startDownloaderEvent(false);
                }
                Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Leaving as contents are null");
                return;
            }

            if (startDownloaderEvent != null)
            {
                Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Called startDownloaderEvent event with TRUE");
                startDownloaderEvent(true);
            }

            isRootContainer = true;

            Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "setting isRootContainer to TRUE");
            Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Contents Length: " + contents.Length);
            foreach (ItemDetails iDetail in contents)
            {
                Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Checking KEY in DB for content url: " + iDetail.szContentUrl + " with SUCCESS");

                string strCheck = dbhandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL, DbHandler.STATUS }, new string[] { iDetail.szContentUrl, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                if (strCheck.Trim().Length == 0)
                {
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "KEY for content url: " + iDetail.szContentUrl + " with SUCCESS not found in DB");
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Creating a new LocalItemDetails with Path: " + iDetail.strName);
                    LocalItemDetails lItem = new LocalItemDetails();
                    lItem.ItemDetails = iDetail;
                    lItem.Path = iDetail.strName;
                    totalFileCount++;
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "totalFileCount: " + totalFileCount + "\n Calling PrepareStructure with lItem");
                    PrepareStructure(lItem);
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "returned from PrepareStructure");
                }
            }

            Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Contents total item count: " + contents[0].nTotalItem);
            for (int n = 0; n < contents[0].nTotalItem; n++)
            {
                if (lockObject.StopThread /*|| refCode != 200*/)
                {
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Stop thread requested Calling CancelAndNotify");
                    CancelAndNotify(CancelReason.USER_CANCEL);
                    break;
                }
                if (contents[n].szItemType == "DIRECTORY")
                {
                    Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Calling analyseItemDetails for DIR: " + contents[n].strName);
                    analyseItemDetails(contents[n], contents[n].strName);
                }
            }

            if (downloadEvent != null)
            {
                Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Calling downloadEvent with TRUE");
                downloadEvent(this, new StructureDownloaderEvent(true));
            }

            Debugger.Instance.logMessage("StructureDownloader - startAnalyseItemDetails", "Leave");
        }

        public void analyseItemDetails(ItemDetails itemDetail,string strPath)
        {
            Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Enter");
            int refCode = 0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(itemDetail.szContentUrl, ref refCode);
            
            if (refCode == ResponseCode.LOGINFAILED1 || refCode == ResponseCode.LOGINFAILED2)
            {
                lockObject.StopThread = true;
                CancelAndNotify(CancelReason.LOGIN_FAILED);
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
                Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Contents Null");
                return;
            }

            foreach (ItemDetails iDetail in contents)
            {
                Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Checking KEY in DB for content url: " + iDetail.szContentUrl + " with SUCCESS");

                string strCheck = dbhandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL, DbHandler.STATUS }, new string[] { iDetail.szContentUrl, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                if (strCheck.Trim().Length == 0)
                {
                    Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "KEY for content url: " + iDetail.szContentUrl + " with SUCCESS not found in DB");
                    Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Creating a new LocalItemDetails with Path: " + iDetail.strName);
                    LocalItemDetails lItem = new LocalItemDetails();
                    lItem.ItemDetails = iDetail;
                    lItem.Path += strPath;
                    lItem.Path += "\\" + iDetail.strName;
                    totalFileCount++;
                    Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "totalFileCount: " + totalFileCount + "\n Calling PrepareStructure with lItem");
                    PrepareStructure(lItem);
                    Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "returned from PrepareStructure");
                }
            }

            Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Contents total item count: " + contents[0].nTotalItem);
            if (contents[0].nTotalItem > 0)
            {
                for (int n = 0; n < contents[0].nTotalItem; n++)
                {
                    if (lockObject.StopThread /*|| refCode != 200*/)
                    {
                        Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Stop thread requested Calling CancelAndNotify");
                        CancelAndNotify(CancelReason.USER_CANCEL);
                        break;
                    }

                    if (contents[n].szItemType == "DIRECTORY")
                    {
                        Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Calling analyseItemDetails for DIR: " + contents[n].strName);
                        analyseItemDetails(contents[n], strPath + "\\" + contents[n].strName);
                    }
                }
            }

            Debugger.Instance.logMessage("StructureDownloader - analyseItemDetails", "Leave");
        }

        private void CancelAndNotify(CancelReason reason)
        {
            Debugger.Instance.logMessage("StructureDownloader - CancelAndNotify", "Enter");
            if(cancelDownloadEvent != null)
            {
                cancelDownloadEvent(reason);
            }
            Debugger.Instance.logMessage("StructureDownloader - CancelAndNotify", "Leave");
        }
    }
}
