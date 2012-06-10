using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Mezeo
{
    public class LocalEvents
    {
        public string FileName { get; set; }

        public string FullPath { get; set; }

        public string OldFileName { get; set; }

        public string OldFullPath { get; set; }

        public EventsType EventType{ get; set; }

        public DateTime EventTimeStamp { get; set; }

        public bool IsDirectory { get; set; }

        public bool IsFile { get; set; }

        public Int32 EventDbId { get; set; }

        public FileAttributes Attributes { get; set; }

        public enum EventsType
        {
            FILE_ACTION_ADDED = 0,
            FILE_ACTION_MODIFIED = 1,
            FILE_ACTION_REMOVED = 2,
            FILE_ACTION_RENAMED = 3,
            FILE_ACTION_MOVE = 4
        }

        public LocalEvents()
        {
            Attributes = FileAttributes.Normal;
            IsDirectory = false;
            IsFile = false;
            EventDbId = -1;
        }
    }
}
