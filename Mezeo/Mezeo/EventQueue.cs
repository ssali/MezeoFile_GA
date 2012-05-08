using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Threading;
//using System.ComponentModel;

namespace Mezeo
{
    public static class EventQueue
    {
        private static Object thisLock = new Object();
        private static List<LocalEvents> eventList = new List<LocalEvents>();
        public static bool QueueNotEmpty()
        {
            return (eventList.Count() > 0);
        }

        public static void Add(LocalEvents newEvent)
        {
            Debugger.Instance.logMessage("EventQueue - Add", "Adding event: (" + newEvent.EventType + ") " + newEvent.FullPath);
            lock (thisLock)
            {
                bool bAdd = true;
                foreach (LocalEvents id in eventList)
                {
                    if (id.FileName == newEvent.FileName)
                    {
                        Debugger.Instance.logMessage("EventQueue - Add", "Local event already exists for: " + newEvent.FullPath);
                        bAdd = false;
                        break;
                    }
                }

                if (bAdd)
                    eventList.Add(newEvent);                
            }
        }

        public static LocalEvents Pop()
        {
            Debugger.Instance.logMessage("EventQueue - Pop", "Popping event.");
            LocalEvents localEvent = null;
            lock (thisLock)
            {
                if (eventList.Count() > 0)
                    localEvent = eventList[0];
                    eventList.RemoveAt(0);
            }
            return localEvent;
        }
    }
}
