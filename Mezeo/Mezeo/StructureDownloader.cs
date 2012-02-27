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

        public delegate void CancelDownLoadEvent();
        public event CancelDownLoadEvent cancelDownloadEvent;

        public delegate void StartDownLoaderEvent(bool bStart);
        public event StartDownLoaderEvent startDownloaderEvent;

        private int totalFileCount = 0;

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
            this.queue = queue;
            this.lockObject = lockObject;
            cRootContainerUrl = rootContainerUrl;
            cFileCloud = fileCloud;
           // Debugger.ShowLogger();
        }

        public void PrepareStructure(LocalItemDetails lItemdDetails)
        {
                lock (lockObject)
                {
                    queue.Enqueue(lItemdDetails);
                    if (isRootContainer)
                    { 
                        Monitor.PulseAll(lockObject);
                    }
                }
        }

        public void startAnalyseItemDetails()
        {
            int refCode=0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(cRootContainerUrl, ref refCode);
            if (contents == null)
            {
                if (downloadEvent != null)
                {
                    downloadEvent(this, new StructureDownloaderEvent(true));
                }
                if (startDownloaderEvent != null)
                {
                    startDownloaderEvent(false);
                }

                return;
            }

            if (startDownloaderEvent != null)
            {
                startDownloaderEvent(true);
            }

            isRootContainer = true;
            LocalItemDetails lItem = new LocalItemDetails();
            lItem.ItemDetails = contents;
            lItem.Path = "";

            PrepareStructure(lItem);
            totalFileCount += contents[0].nTotalItem;


            for (int n = 0; n < contents[0].nTotalItem; n++)
            {
                if (lockObject.StopThread)
                {
                    CancelAndNotify();
                    break;
                }
                if (contents[n].szItemType == "DIRECTORY")
                {
                    analyseItemDetails(contents[n],lItem.Path);
                }
            }

            if (downloadEvent != null)
            {
                downloadEvent(this, new StructureDownloaderEvent(true));
            }
        }

        public void analyseItemDetails(ItemDetails itemDetail,string strPath)
        {
            int refCode = 0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(itemDetail.szContentUrl, ref refCode);

            if (contents == null)
            {
                return;
            }

            LocalItemDetails lItem = new LocalItemDetails();
            lItem.ItemDetails = contents;
            strPath += itemDetail.strName;
            lItem.Path = strPath;
           
            totalFileCount += contents[0].nTotalItem;
           
            PrepareStructure(lItem);

            if (contents[0].nTotalItem > 0)
            {
                for (int n = 0; n < contents[0].nTotalItem; n++)
                {
                    if (lockObject.StopThread)
                    {
                        CancelAndNotify();
                        break;
                    }

                    if (contents[n].szItemType == "DIRECTORY")
                        analyseItemDetails(contents[n], strPath + "\\");
                }
            }
        }

        private void CancelAndNotify()
        {
            if(cancelDownloadEvent != null)
            {
                cancelDownloadEvent();
            }
        }

    }
}
