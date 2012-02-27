using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    class LocalEvents
    {
        public string FileName { get; set; }

        public string FullPath { get; set; }

        public string OldFileName { get; set; }

        public string OldFullPath { get; set; }

        public EventsType EventType{ get; set; }

        public DateTime EventTimeStamp { get; set; }

        public enum EventsType
        {
            FILE_ACTION_ADDED,
            FILE_ACTION_MODIFIED,
            FILE_ACTION_REMOVED,
            FILE_ACTION_RENAMED
        }

    }
}
