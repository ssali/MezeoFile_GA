using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mezeo
{
    public static class EventQueue
    {
        private static Object thisLock = new Object();
        private static List<LocalEvents> eventList = new List<LocalEvents>();

        public static void Add(LocalEvents newEvent)
        {
            lock (thisLock)
            {
                bool bAdd = true;
                foreach (LocalEvents id in eventList)
                {
                    if (id.FileName == newEvent.FileName)
                    {
                        bAdd = false;
                        break;
                    }
                }

                if (bAdd)
                    eventList.Add(newEvent);                
            }
        }
    }
}
