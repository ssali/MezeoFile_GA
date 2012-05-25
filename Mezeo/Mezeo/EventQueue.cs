using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

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
        public delegate void WatchCompleted();
        public static event WatchCompleted WatchCompletedEvent;

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
            bool bNewEventExists = false;
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
                        lEvent.EventTimeStamp = id.EventTimeStamp;
                        eventList.Add(lEvent);
                        eventsToRemove.Add(id);
                        bNewEventExists = true;
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
            if (bNewEventExists)
            {
                if (WatchCompletedEvent != null)
                {
                    if (EventQueue.QueueNotEmpty())
                        WatchCompletedEvent();
                }
            }
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
            int indexToRemove = -1;

            LogWrapper.LogMessage("EventQueue - Add", "Adding event: (" + newEvent.EventType + ") " + newEvent.FullPath);
            if (newEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
            {
                LogWrapper.LogMessage("EventQueue - Add", "              (" + newEvent.EventType + ") old path:" + newEvent.OldFullPath);
                LogWrapper.LogMessage("EventQueue - Add", "              (" + newEvent.EventType + ") new path:" + newEvent.FullPath);
            }

            lock (thisLock)
            {
                bool bAdd = true;

                // Only FILE_ACTION_MODIFIED events are possibly ignored, so they are the only ones that
                // we should spend the effort/time to loop through the list for.
                if (newEvent.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED)
                {
                    bool isFile = File.Exists(newEvent.FullPath);

                    foreach (LocalEvents id in eventListCandidates)
                    {
                        if (id.FileName == newEvent.FileName)
                        {
                            if (isFile)
                            {
                                // If a event type is added, removed, renamed, or moved, then go ahead and accept the event.
                                // If the new event is MODIFIED and the existing is ADDED, then update the timestamp of the
                                // existing event, but don't add it to the list.
                                if ((id.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED) ||
                                    (id.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED))
                                {
                                    LogWrapper.LogMessage("EventQueue - Add", "Local event already exists for: " + newEvent.FullPath);
                                    id.EventTimeStamp = newEvent.EventTimeStamp;
                                    bAdd = false;
                                    break;
                                }
                            }
                            else
                            {
                                // For directories, ignore any FILE_ACTION_MODIFIED events.  They should not reset the time either.
                                LogWrapper.LogMessage("EventQueue - Add", "Local event already exists for: " + newEvent.FullPath);
                                bAdd = false;
                                break;
                            }
                        }
                    }
                }
                else if (newEvent.EventType == LocalEvents.EventsType.FILE_ACTION_RENAMED)
                {
                    bool isFile = File.Exists(newEvent.FullPath);
                    if (isFile)
                    {
                        // If something locally has been renamed, then look through the existing events
                        // and see if there is a 'created' event for the old path.  If so, then the
                        // existing event MUST be changed so the path reflects the new path.  Otherwise
                        // no file will be uploaded (the local file has been renamed) and the rename
                        // won't occur on the server since the file wasn't uploaded.
                        bool foundAddEvent = false;
                        foreach (LocalEvents id in eventListCandidates)
                        {
                            if ((id.FileName == newEvent.OldFileName) && (id.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED))
                            {
                                // Set the path to the new path so the correct folder is created or file is uploaded.
                                // Set the event to FILE_ACTION_MODIFIED so that a new version of the file is uploaded.
                                LogWrapper.LogMessage("EventQueue - Add", "Local ADDED event already exists for: " + newEvent.FullPath);
                                LogWrapper.LogMessage("EventQueue - Add", "Changing existing path from " + id.FullPath + " to " + newEvent.FullPath);
                                LogWrapper.LogMessage("EventQueue - Add", "Changing existing event from " + id.EventType + " to FILE_ACTION_MODIFIED");
                                id.FullPath = newEvent.FullPath;
                                id.FileName = newEvent.FileName;
                                id.EventType = LocalEvents.EventsType.FILE_ACTION_MODIFIED;
                                bAdd = false;
                                foundAddEvent = true;
                            }
                            if (foundAddEvent)
                            {
                                // If a FILE_ACTION_ADDED event was modified, then we keep looking for a FILE_ACTION_REMOVED
                                // action as well and remove/change it to something else.
                                if ((id.FileName == newEvent.FileName) && (id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED))
                                {
                                    // The delete action for the file should be deleted since it would delete the file
                                    // that will now be uploaded.
                                    indexToRemove = eventListCandidates.IndexOf(id);
                                    LogWrapper.LogMessage("EventQueue - Add", "Removing delete action for: " + newEvent.FullPath);
                                    break;
                                }
                            }
                        }
                    }
                }

                if (-1 != indexToRemove)
                {
                    eventListCandidates.RemoveAt(indexToRemove);
                }

                if (bAdd)
                {
                    eventListCandidates.Add(newEvent);
                }
            }
        }

        public static LocalEvents Pop()
        {
            LogWrapper.LogMessage("EventQueue - Pop", "Popping event.");
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
