using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using MezeoFileSupport;

namespace Mezeo
{
    class FileDownloader
    {
        Queue<LocalItemDetails> queue;
        Object lockObject;
        MezeoFileCloud cFileCloud;
        DbHandler dbhandler = new DbHandler();

        public FileDownloader(Queue<LocalItemDetails> queue, Object lockObject, MezeoFileCloud fileCloud)
        {
            this.queue = queue;
            this.lockObject = lockObject;
            cFileCloud = fileCloud;

            dbhandler.OpenConnection();
        }

        public void consume()
        {
            LocalItemDetails itemDetail;
            while (true)
            {
                lock (lockObject)
                {
                    if (queue.Count == 0)
                    { 
                        Monitor.Wait(lockObject);
                        continue; 
                    }
                    itemDetail = queue.Dequeue();
                    foreach (ItemDetails id in itemDetail.ItemDetails)
                    {
                        if (id.szItemType == "DIRECTORY")
                        {
                            if (itemDetail.Path.Length != 0)
                              System.IO.Directory.CreateDirectory(BasicInfo.SyncDirPath + "\\" + itemDetail.Path +"\\" + id.strName);
                            else
                                System.IO.Directory.CreateDirectory(BasicInfo.SyncDirPath + "\\" + id.strName);
                        }
                        else
                        {
                             int refCode = 0;
                             cFileCloud.DownloadFile(id.szContentUrl + '/' + id.strName,
                                                     BasicInfo.SyncDirPath + "\\" + itemDetail.Path + "\\" + id.strName, ref refCode);

                             id.strETag = cFileCloud.GetETag(id.szContentUrl, ref refCode);
                        }

                        //dbhandler.
                    }
                }
            }
        }

    }
}
