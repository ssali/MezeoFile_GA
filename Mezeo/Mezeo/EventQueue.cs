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

        public static void FillInFileInfo(ref LocalEvents theEvent)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(theEvent.FullPath);
                if (fileInfo.Exists)
                {
                    theEvent.Attributes = fileInfo.Attributes;
                    if (0 == (fileInfo.Attributes & FileAttributes.Directory))
                    {
                        theEvent.IsDirectory = false;
                        theEvent.IsFile = true;
                    }
                    else
                    {
                        theEvent.IsDirectory = true;
                        theEvent.IsFile = false;
                    }
                }
                else
                {
                    theEvent.IsDirectory = true;
                    theEvent.IsFile = false;
                }
            }
            catch (Exception ex)
            {
                LogWrapper.LogMessage("EventQueue - FillInFileInfo", "Caught exception: " + ex.Message);
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

            // Only fill in this information if the event isn't a DELETE event.
            // Otherwise, the file/folder isn't there to get the info for.
            if (newEvent.EventType != LocalEvents.EventsType.FILE_ACTION_REMOVED)
            {
                FillInFileInfo(ref newEvent);
            }

            lock (thisLock)
            {
                bool bAdd = true;

                // Only FILE_ACTION_MODIFIED events are possibly ignored, so they are the only ones that
                // we should spend the effort/time to loop through the list for.
                if (newEvent.EventType == LocalEvents.EventsType.FILE_ACTION_MODIFIED)
                {
                    foreach (LocalEvents id in eventListCandidates)
                    {
                        if (id.FileName == newEvent.FileName)
                        {
                            if (newEvent.IsFile)
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
                    if (newEvent.IsFile)
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
                else if (newEvent.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED)
                {
                    // The 'Save As' from editors like Notepad execute a strange series of events.  Specifically,
                    // FILE_ACTION_ADDED, FILE_ACTION_REMOVED, FILE_ACTION_ADDED, and FILE_ACTION_MODIFIED.
                    // If the ADDED action finds both the ADDED and REMOVED actions earlier in the list, then
                    // modify the first ADDED event, remove the REMOVED, and throw this event away.
                    bool foundAdded = false;
                    bool foundRemoved = false;
                    int indexAdded = -1;
                    int indexRemoved = -1;
                    foreach (LocalEvents id in eventListCandidates)
                    {
                        if ((id.EventType == LocalEvents.EventsType.FILE_ACTION_ADDED) && (id.FullPath == newEvent.FullPath))
                        {
                            LogWrapper.LogMessage("EventQueue - Add", "Found existing FILE_ACTION_ADDED (save seq) for: " + newEvent.FullPath);
                            foundAdded = true;
                            indexAdded = eventListCandidates.IndexOf(id);
                        }
                        if (foundAdded)
                        {
                            if ((id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED) && (id.FullPath == newEvent.FullPath))
                            {
                                LogWrapper.LogMessage("EventQueue - Add", "Found existing FILE_ACTION_REMOVED (save seq) for: " + newEvent.FullPath);
                                foundRemoved = true;
                                indexRemoved = eventListCandidates.IndexOf(id);
                            }
                        }

                        // We found what we were looking for so we can exit the loop.
                        if (foundAdded && foundRemoved)
                            break;
                    }

                    if (foundAdded && foundRemoved)
                    {
                        // If I found the two events (in the correct order), then I need to modify
                        // the Added entry, remove the Removed entry, and throw this entry away.
                        bAdd = false;
                        indexToRemove = indexRemoved;
                        eventListCandidates[indexAdded].EventTimeStamp = newEvent.EventTimeStamp;
                        eventListCandidates[indexAdded].IsDirectory = newEvent.IsDirectory;
                        eventListCandidates[indexAdded].IsFile = newEvent.IsFile;
                        eventListCandidates[indexAdded].Attributes = newEvent.Attributes;
                        LogWrapper.LogMessage("EventQueue - Add", "Updating existing FILE_ACTION_ADD for: " + newEvent.FullPath);
                        LogWrapper.LogMessage("EventQueue - Add", "Removing existing FILE_ACTION_REMOVED for: " + newEvent.FullPath);
                        LogWrapper.LogMessage("EventQueue - Add", "Ignoring new FILE_ACTION_ADDED for: " + newEvent.FullPath);
                    }
                    //else
                    //{
                    //    string fileName = newEvent.FileName.Substring(newEvent.FileName.LastIndexOf("\\") + 1);

                    //    foundRemoved = false;
                    //    indexRemoved = -1;
                    //    LogWrapper.LogMessage("EventQueue - Add", "Looking for a MOVE sequence.");
                    //    foreach (LocalEvents id in eventListCandidates)
                    //    {
                    //        LogWrapper.LogMessage("EventQueue - Add", "Comparing " + id.FileName + " to " + fileName);
                    //        //if ((id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED) && (id.FileName == newEvent.FileName))
                    //        if ((id.EventType == LocalEvents.EventsType.FILE_ACTION_REMOVED) && (id.FileName == fileName))
                    //        {
                    //            LogWrapper.LogMessage("EventQueue - Add", "Found existing FILE_ACTION_REMOVED (move seq) for: " + newEvent.FullPath);
                    //            indexRemoved = eventListCandidates.IndexOf(id);
                    //            foundRemoved = true;

                    //            // We found what we were looking for so we can exit the loop.
                    //            break;
                    //        }
                    //    }

                    //    if (foundRemoved)
                    //    {
                    //        // If an item has an ADDED and REMOVED action within a VERY short time
                    //        // period then the item was probably MOVED/RENAMED.  Adjust the existing
                    //        // REMOVED action accordingly and ignore the ADDED action.
                    //        TimeSpan diff = newEvent.EventTimeStamp - eventListCandidates[indexRemoved].EventTimeStamp;

                    //        LogWrapper.LogMessage("EventQueue - Add", "Time diff (move seq) is : " + diff.TotalMilliseconds + "ms");
                    //        if (1 >= diff.TotalMilliseconds)
                    //        {
                    //            LogWrapper.LogMessage("EventQueue - Add", "Time diff is small so ASSuming " + eventListCandidates[indexRemoved].FullPath + " was MOVED to " + newEvent.FullPath);
                    //            // Change the REMOVED event to a MOVE.
                    //            eventListCandidates[indexRemoved].EventType = LocalEvents.EventsType.FILE_ACTION_MOVE;

                    //            // Update the old path, full path, and time.
                    //            eventListCandidates[indexRemoved].OldFileName = eventListCandidates[indexRemoved].FileName;
                    //            eventListCandidates[indexRemoved].FileName = newEvent.FileName;
                    //            eventListCandidates[indexRemoved].OldFullPath = eventListCandidates[indexRemoved].FullPath;
                    //            eventListCandidates[indexRemoved].FullPath = newEvent.FullPath;

                    //            // Ignore this event since it's really a move.
                    //            bAdd = false;

                    //            LogWrapper.LogMessage("EventQueue - Add", "Updating existing FILE_ACTION_REMOVED for: " + eventListCandidates[indexRemoved].FullPath);
                    //            LogWrapper.LogMessage("EventQueue - Add", "Ignoring new FILE_ACTION_ADDED for: " + newEvent.FullPath);
                    //        }
                    //    }
                    //}
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
