using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

namespace Mezeo
{
    class Watcher
    {
        Object lockObject;
        FileSystemWatcher fileWatcher;
        string folderToWatch;
        DateTime eventTime;
        System.Timers.Timer timer;

        public delegate void WatchCompleted();
        public event WatchCompleted WatchCompletedEvent;

        public Watcher(Object lockObject, string folder)
        {
            this.lockObject = lockObject;
            this.folderToWatch = folder;
            fileWatcher = new FileSystemWatcher(folderToWatch);
            
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Filter = "*.*";
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.LastWrite | NotifyFilters.DirectoryName | NotifyFilters.FileName;
            
            fileWatcher.Changed += new FileSystemEventHandler(fileWatcher_Changed);
            fileWatcher.Created += new FileSystemEventHandler(fileWatcher_Created);
            fileWatcher.Deleted += new FileSystemEventHandler(fileWatcher_Deleted);
            fileWatcher.Renamed += new RenamedEventHandler(fileWatcher_Renamed);
            //eventTime = DateTime.Now;
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
            timer.Interval = 2 * 1000;
            timer.AutoReset = false;
        }

        void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (WatchCompletedEvent != null)
            {
                if (EventQueue.QueueNotEmpty())
                    WatchCompletedEvent();
            }
        }

        public void StartMonitor()
        {
            fileWatcher.EnableRaisingEvents = true;
        }

        public void StopMonitor()
        {
            fileWatcher.EnableRaisingEvents = false;
            fileWatcher.Dispose();
        }

        private void StampTime()
        {
            eventTime = DateTime.Now;
            timer.Stop();
            timer.Start();
        }

        void fileWatcher_Renamed(object sender, RenamedEventArgs e)
        {
            StampTime();
            FillEventsQueue(e, true, LocalEvents.EventsType.FILE_ACTION_RENAMED);
            
        }

        void fileWatcher_Deleted(object sender, FileSystemEventArgs e)
        {
            StampTime();
            FillEventsQueue(e, false, LocalEvents.EventsType.FILE_ACTION_REMOVED);           
        }

        void fileWatcher_Created(object sender, FileSystemEventArgs e)
        {
            StampTime();
            FillEventsQueue(e, false, LocalEvents.EventsType.FILE_ACTION_ADDED);            
        }

        void fileWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            StampTime();
            FillEventsQueue(e, false, LocalEvents.EventsType.FILE_ACTION_MODIFIED);            
        }

        public void FillEventsQueue(EventArgs e,bool isRename, LocalEvents.EventsType eventType)
        {
            LocalEvents lEvent=new LocalEvents();
            
            if(isRename)
            {
                RenamedEventArgs rArgs=(RenamedEventArgs)e;
                lEvent.FileName = rArgs.Name;
                lEvent.FullPath = rArgs.FullPath;
                lEvent.OldFileName = rArgs.OldName;
                lEvent.OldFullPath = rArgs.OldFullPath;
                lEvent.EventType = eventType;
                lEvent.EventTimeStamp = eventTime;
            }
            else
            {
                FileSystemEventArgs rArgs = (FileSystemEventArgs)e;
                lEvent.FileName = rArgs.Name;
                lEvent.FullPath = rArgs.FullPath;
                lEvent.OldFileName = "";
                lEvent.OldFullPath = "";
                lEvent.EventType = eventType;
                lEvent.EventTimeStamp = eventTime;
            }

            EventQueue.Add(lEvent);
        }
    }
}
