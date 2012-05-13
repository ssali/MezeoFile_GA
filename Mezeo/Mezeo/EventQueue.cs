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
        // Time without receiving events for a resource before
        // it is moved from evenListCandidates to eventList.
        private static int TIME_WITHOUT_EVENTS = 20000;  // 20 seconds as milliseconds.

        // A list of events that have not had any activity in the last TIME_WITHOUT_EVENTS.
        private static List<LocalEvents> eventList = new List<LocalEvents>();

        // A list of events for resources that are still receiving/generating events.
        private static List<LocalEvents> eventListCandidates = new List<LocalEvents>();

        private static Object thisLock = new Object();
        private static System.Timers.Timer timer;

        public static void InitEventQueue()
        {
            timer = new System.Timers.Timer();
            timer.Elapsed += new System.Timers.ElapsedEventHandler(EventQueue.timer_Elapsed);
            timer.Interval = TIME_WITHOUT_EVENTS;
            timer.AutoReset = true;
            timer.Enabled = true;
        }

        public static void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            //bool bNewEventExists = false;
            DateTime currTime = DateTime.Now;
            List<LocalEvents> eventsToRemove = new List<LocalEvents>();

            // Check the event candidate list and see which events should be moved.
            lock (thisLock)
            {
                // If the resource has not had an event in the last X timespan,
                // move it from the eventListCandidates to eventList.
                foreach (LocalEvents id in eventListCandidates)
                {
                    TimeSpan diff = currTime - id.EventTimeStamp;
                    if (TIME_WITHOUT_EVENTS <= diff.TotalMilliseconds)
                    {
                        LocalEvents lEvent = new LocalEvents();
                        lEvent = id;
                        eventList.Add(lEvent);
                        eventsToRemove.Add(id);
                        //bNewEventExists = true;
                    }
                }

                // Each event that was moved to eventList must
                // be removed from eventListCandidates.
                foreach (LocalEvents id in eventsToRemove)
                {
                    eventListCandidates.Remove(id);
                }

                eventsToRemove.Clear();
            }

            // If something was added to the list, trigger the event.
            //if (bNewEventExists)
            //{
            //    if (EventQueue.QueueNotEmpty())
            //        WatchCompletedEvent();
            //}
        }

        public static bool QueueNotEmpty()
        {
            bool bIsEmpty = true;
            lock (thisLock)
            {
                bIsEmpty = (eventList.Count() > 0);
            }
            return bIsEmpty;
        }

        public static int QueueCount()
        {
            int queueCount = 0;
            lock (thisLock)
            {
                queueCount = eventList.Count();
            }
            return queueCount;
        }

        public static List<LocalEvents> GetCurrentQueue()
        {
            lock (thisLock)
            {
                List<LocalEvents> currentList = eventList;
                eventList = new List<LocalEvents>();
                return currentList;
            }
        }

        public static void Add(LocalEvents newEvent)
        {
            Debugger.Instance.logMessage("EventQueue - Add", "Adding event: (" + newEvent.EventType + ") " + newEvent.FullPath);
            lock (thisLock)
            {
                bool bAdd = true;
                foreach (LocalEvents id in eventListCandidates)
                //foreach (LocalEvents id in eventList)
                {
                    if (id.FileName == newEvent.FileName)
                    {
                        Debugger.Instance.logMessage("EventQueue - Add", "Local event already exists for: " + newEvent.FullPath);
                        id.EventTimeStamp = newEvent.EventTimeStamp;
                        bAdd = false;
                        break;
                    }
                }

                if (bAdd)
                {
                    eventListCandidates.Add(newEvent);
                    //eventList.Add(newEvent);
                }
            }
        }

        public static LocalEvents Pop()
        {
            Debugger.Instance.logMessage("EventQueue - Pop", "Popping event.");
            LocalEvents localEvent = null;
            lock (thisLock)
            {
                if (eventList.Count() > 0)
                {
                    localEvent = eventList[0];
                    eventList.RemoveAt(0);
                }
            }
            return localEvent;
        }
    }
}
