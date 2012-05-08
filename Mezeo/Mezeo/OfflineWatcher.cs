using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

namespace Mezeo
{
    class OfflineWatcher
    {
        List<string> currentStructure = new List<string>();
       // List<LocalEvents> collectedLocalEvent = new List<LocalEvents>();
        DbHandler dbHandler;
     

        public OfflineWatcher(DbHandler dbHandler)
        {
            this.dbHandler = dbHandler;
                       
        }

        public List<LocalEvents> PrepareStructureList()
        {
            foreach(string dirs in Directory.GetDirectories(BasicInfo.SyncDirPath, "*.*", SearchOption.AllDirectories))
            {
                string name = dirs.Substring(BasicInfo.SyncDirPath.Length + 1);
                currentStructure.Add(name);
            }

            foreach (string files in Directory.GetFiles(BasicInfo.SyncDirPath, "*.*", SearchOption.AllDirectories))
            {
                string name = files.Substring(BasicInfo.SyncDirPath.Length + 1);
                currentStructure.Add(name);
            }

            List<string> dbKeys = dbHandler.GetKeyList();

            foreach (string localKeys in currentStructure)
            {
                FileInfo fInfo = null;
                DirectoryInfo dInfo = null;
                bool IsFile = false;

                if (Directory.Exists(BasicInfo.SyncDirPath + "\\" + localKeys))
                {
                    dInfo = new DirectoryInfo(BasicInfo.SyncDirPath + "\\" + localKeys);
                    if ((dInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || (dInfo.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary)
                        continue;
                }
                else
                {
                    IsFile = true;
                    fInfo = new FileInfo(BasicInfo.SyncDirPath + "\\" + localKeys);
                    if ((fInfo.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden || (fInfo.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary)
                        continue;
                }

                string strCheck = dbHandler.GetString(DbHandler.TABLE_NAME, DbHandler.CONTENT_URL, new string[] { DbHandler.KEY, DbHandler.STATUS }, new string[] { localKeys, "SUCCESS" }, new System.Data.DbType[] { System.Data.DbType.String, System.Data.DbType.String });
                //if (dbKeys.Contains(localKeys))
                if(strCheck.Trim().Length != 0)
                {
                    DateTime dbModDate = dbHandler.GetDateTime(DbHandler.TABLE_NAME, DbHandler.MODIFIED_DATE , DbHandler.KEY , localKeys);
                    
                    bool isModified = false;

                    if (IsFile)
                    {                        
                        DateTime lastWriteTime = fInfo.LastWriteTime;
                        lastWriteTime = lastWriteTime.AddMilliseconds(-lastWriteTime.Millisecond);
                        
                        TimeSpan diff = lastWriteTime - dbModDate;

                        if (diff >= TimeSpan.FromSeconds(1))
                        {
                            isModified = true;
                        }
                    }
                    //else
                    //{
                    //    if (dInfo.LastWriteTime > dbModDate)
                    //    {
                    //        isModified = true;
                    //    }
                    //}


                    if (isModified)
                    {
                        LocalEvents lEvent = new LocalEvents();
                        lEvent.FileName = localKeys;
                        if (IsFile)
                            lEvent.FullPath = fInfo.FullName;
                        else
                            lEvent.FullPath = dInfo.FullName;

                        lEvent.OldFileName = "";
                        lEvent.OldFullPath = "";
                        lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_MODIFIED;

                        //collectedLocalEvent.Add(lEvent);
                        EventQueue.Add(lEvent);
                        
                       
                    }
                }
                else
                {
                    LocalEvents lEvent = new LocalEvents();
                    lEvent.FileName = localKeys;
                    if (IsFile)
                        lEvent.FullPath = fInfo.FullName;
                    else
                        lEvent.FullPath = dInfo.FullName;

                    lEvent.OldFileName = "";
                    lEvent.OldFullPath = "";
                    lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_ADDED;

                    //collectedLocalEvent.Add(lEvent);
                    EventQueue.Add(lEvent);
                }
            }

            foreach (string key in dbKeys)
            {
                FileInfo fInfo = null;
                DirectoryInfo dInfo = null;
                bool IsFile = false;

                if (Directory.Exists(BasicInfo.SyncDirPath + "\\" + key))
                {
                    dInfo = new DirectoryInfo(BasicInfo.SyncDirPath + "\\" + key);
                }
                else
                {
                    IsFile = true;
                    fInfo = new FileInfo(BasicInfo.SyncDirPath + "\\" + key);
                }

                if(!currentStructure.Contains(key))
                {
                    LocalEvents lEvent = new LocalEvents();
                    lEvent.FileName = key;
                    if (IsFile)
                        lEvent.FullPath = fInfo.FullName;
                    else
                        lEvent.FullPath = dInfo.FullName;

                    lEvent.OldFileName = "";
                    lEvent.OldFullPath = "";
                    lEvent.EventType = LocalEvents.EventsType.FILE_ACTION_REMOVED;

                    //collectedLocalEvent.Add(lEvent);
                    EventQueue.Add(lEvent);

                }
            }

            return new List<LocalEvents>();
        }
    }
}
