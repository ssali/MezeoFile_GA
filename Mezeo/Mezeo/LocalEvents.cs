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

        public FileAttributes Attributes { get; set; }

        public enum EventsType
        {
            FILE_ACTION_ADDED,
            FILE_ACTION_MODIFIED,
            FILE_ACTION_REMOVED,
            FILE_ACTION_RENAMED,
            FILE_ACTION_MOVE
        }

        public LocalEvents()
        {
            Attributes = FileAttributes.Normal;
            IsDirectory = false;
            IsFile = false;
        }
    }
}
