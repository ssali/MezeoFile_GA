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
        Debugger debugger = new Debugger();
        public delegate void StructureDownloadEvent(object sender, StructureDownloaderEvent e);
        public event StructureDownloadEvent downloadEvent;

        public delegate void CancelDownLoadEvent();
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
        MezeoFileCloud cFileCloud;//=new MezeoFileCloud();

        bool isRootContainer = false;

        static int seq = 0;
        public StructureDownloader(Queue<LocalItemDetails> queue, ThreadLockObject lockObject, string rootContainerUrl, MezeoFileCloud fileCloud)
        {
            debugger.logMessage("StructureDownloader - Constructor", "Enter");
            this.queue = queue;
            this.lockObject = lockObject;
            cRootContainerUrl = rootContainerUrl;
            cFileCloud = fileCloud;
            dbhandler.OpenConnection();

            debugger.logMessage("StructureDownloader - Constructor Call", "Content Url: " + rootContainerUrl);
            debugger.logMessage("StructureDownloader - Constructor", "Leave");
           // debugger.ShowLogger();
        }

        public void PrepareStructure(LocalItemDetails lItemdDetails)
        {
            debugger.logMessage("StructureDownloader - PrepareStructure", "Enter");

                lock (lockObject)
                {
                    queue.Enqueue(lItemdDetails);
                    if (isRootContainer)
                    {
                        debugger.logMessage("StructureDownloader - PrepareStructure", "Pulse");
                        Monitor.PulseAll(lockObject);
                    }
                }

                debugger.logMessage("StructureDownloader - PrepareStructure", "Leave");
        }

        public void startAnalyseItemDetails()
        {
            debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Enter");

            int refCode=0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(cRootContainerUrl, ref refCode);

            //if (refCode != 200)
            //{
            //    CancelAndNotify();
            //}

            if (contents == null)
            {
                debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Contents Null");

                if (downloadEvent != null)
                {

                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Called download event with TRUE");
                    downloadEvent(this, new StructureDownloaderEvent(true));
                }

                if (startDownloaderEvent != null)
                {
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Called startDownloaderEvent event with FALSE");
                    startDownloaderEvent(false);
                }
                debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Leaving as contents are null");
                return;
            }

            if (startDownloaderEvent != null)
            {
                debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Called startDownloaderEvent event with TRUE");
                startDownloaderEvent(true);
            }

            isRootContainer = true;

            debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "setting isRootContainer to TRUE");
            debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Contents Length: " + contents.Length);
            foreach (ItemDetails iDetail in contents)
            {
                debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Checking KEY in DB for content url: " + iDetail.szContentUrl + " with SUCCESS");

                string strCheck = dbhandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL, DbHandler.STATUS }, new string[] { iDetail.szContentUrl, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                if (strCheck.Trim().Length == 0)
                {
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "KEY for content url: " + iDetail.szContentUrl + " with SUCCESS not found in DB");
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Creating a new LocalItemDetails with Path: " + iDetail.strName);
                    LocalItemDetails lItem = new LocalItemDetails();
                    lItem.ItemDetails = iDetail;
                    lItem.Path = iDetail.strName;
                    totalFileCount++;
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "totalFileCount: " + totalFileCount + "\n Calling PrepareStructure with lItem");
                    PrepareStructure(lItem);
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "returned from PrepareStructure");
                }
            }

            debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Contents total item count: " + contents[0].nTotalItem);
            for (int n = 0; n < contents[0].nTotalItem; n++)
            {
                if (lockObject.StopThread /*|| refCode != 200*/)
                {
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Stop thread requested Calling CancelAndNotify");
                    CancelAndNotify();
                    break;
                }
                if (contents[n].szItemType == "DIRECTORY")
                {
                    debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Calling analyseItemDetails for DIR: " + contents[n].strName);
                    analyseItemDetails(contents[n], contents[n].strName);
                }
            }

            if (downloadEvent != null)
            {
                debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Calling downloadEvent with TRUE");
                downloadEvent(this, new StructureDownloaderEvent(true));
            }

            debugger.logMessage("StructureDownloader - startAnalyseItemDetails", "Leave");
        }

        public void analyseItemDetails(ItemDetails itemDetail,string strPath)
        {
            debugger.logMessage("StructureDownloader - analyseItemDetails", "Enter");
            int refCode = 0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(itemDetail.szContentUrl, ref refCode);

            //if (refCode != 200)
            //{
            //    CancelAndNotify();
            //}

            if (contents == null)
            {
                debugger.logMessage("StructureDownloader - analyseItemDetails", "Contents Null");
                return;
            }

            foreach (ItemDetails iDetail in contents)
            {
                debugger.logMessage("StructureDownloader - analyseItemDetails", "Checking KEY in DB for content url: " + iDetail.szContentUrl + " with SUCCESS");

                string strCheck = dbhandler.GetString(DbHandler.TABLE_NAME, DbHandler.KEY, new string[] { DbHandler.CONTENT_URL, DbHandler.STATUS }, new string[] { iDetail.szContentUrl, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                if (strCheck.Trim().Length == 0)
                {
                    debugger.logMessage("StructureDownloader - analyseItemDetails", "KEY for content url: " + iDetail.szContentUrl + " with SUCCESS not found in DB");
                    debugger.logMessage("StructureDownloader - analyseItemDetails", "Creating a new LocalItemDetails with Path: " + iDetail.strName);
                    LocalItemDetails lItem = new LocalItemDetails();
                    lItem.ItemDetails = iDetail;
                    lItem.Path += strPath;
                    lItem.Path += "\\" + iDetail.strName;
                    totalFileCount++;
                    debugger.logMessage("StructureDownloader - analyseItemDetails", "totalFileCount: " + totalFileCount + "\n Calling PrepareStructure with lItem");
                    PrepareStructure(lItem);
                    debugger.logMessage("StructureDownloader - analyseItemDetails", "returned from PrepareStructure");
                }
            }

            debugger.logMessage("StructureDownloader - analyseItemDetails", "Contents total item count: " + contents[0].nTotalItem);
            if (contents[0].nTotalItem > 0)
            {
                for (int n = 0; n < contents[0].nTotalItem; n++)
                {
                    if (lockObject.StopThread /*|| refCode != 200*/)
                    {
                        debugger.logMessage("StructureDownloader - analyseItemDetails", "Stop thread requested Calling CancelAndNotify");
                        CancelAndNotify();
                        break;
                    }

                    if (contents[n].szItemType == "DIRECTORY")
                    {
                        debugger.logMessage("StructureDownloader - analyseItemDetails", "Calling analyseItemDetails for DIR: " + contents[n].strName);
                        analyseItemDetails(contents[n], strPath + "\\" + contents[n].strName);
                    }
                }
            }

            debugger.logMessage("StructureDownloader - analyseItemDetails", "Leave");
        }

        private void CancelAndNotify()
        {
            debugger.logMessage("StructureDownloader - CancelAndNotify", "Enter");
            if(cancelDownloadEvent != null)
            {
                cancelDownloadEvent();
            }
            debugger.logMessage("StructureDownloader - CancelAndNotify", "Leave");
        }

    }
}
