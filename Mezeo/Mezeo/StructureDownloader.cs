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
        Queue<LocalItemDetails> queue;
        Object lockObject;
        string cRootContainerUrl;
        MezeoFileCloud cFileCloud;//=new MezeoFileCloud();

        bool isRootContainer = false;

        static int seq = 0;
        public StructureDownloader(Queue<LocalItemDetails> queue, Object lockObject, string rootContainerUrl, MezeoFileCloud fileCloud)
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
            isRootContainer = true;
            LocalItemDetails lItem = new LocalItemDetails();
            lItem.ItemDetails = contents;
            lItem.Path = "";

            PrepareStructure(lItem);

            for (int n = 0; n < contents[0].nTotalItem; n++)
            {
                if (contents[n].szItemType == "DIRECTORY")
                {
                    analyseItemDetails(contents[n],lItem.Path);
                }
            }
        }

        public void analyseItemDetails(ItemDetails itemDetail,string strPath)
        {
            int refCode = 0;
            ItemDetails[] contents = cFileCloud.DownloadItemDetails(itemDetail.szContentUrl, ref refCode);
            LocalItemDetails lItem = new LocalItemDetails();
            lItem.ItemDetails = contents;
            strPath += itemDetail.strName;
            lItem.Path = strPath;

            PrepareStructure(lItem);
            if (contents[0].nTotalItem > 0)
            {
                for (int n = 0; n < contents[0].nTotalItem; n++)
                {
                    if (contents[n].szItemType == "DIRECTORY")
                        analyseItemDetails(contents[n], strPath + "\\");
                }
            }
        }

    }
}
