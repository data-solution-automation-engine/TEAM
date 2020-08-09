using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace TEAM
{
    /// <summary>
    /// An event log is a list of events.
    /// </summary>
    public class EventLog : List<Event>
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public EventLog()
        {
            //EventList = null;
        }

        /// <summary>
        /// Merge another event log into the class.
        /// </summary>
        /// <param name="otherEventLog"></param>
        public void MergeEventLog(EventLog otherEventLog)
        {
            foreach (Event mergeEvent in otherEventLog)
            {
                this.Add(mergeEvent);
            }
        }

        /// <summary>
        /// Spool the Event Log to disk.
        /// </summary>
        /// <param name="targetFile"></param>
        public void SaveEventLogToFile(string targetFile)
        {
            StringBuilder output = new StringBuilder();

            try
            {
                //Output to file
                using (var outfile = new StreamWriter(targetFile))
                {
                    foreach (Event localEvent in this)
                    {
                        output.AppendLine($"{localEvent.eventTime} - {localEvent.eventCode} : {localEvent.eventDescription}");
                    }

                    outfile.Write(output.ToString());
                    outfile.Close();
                }
                
                this.Add(Event.CreateNewEvent(EventTypes.Information, "The file was successfully saved to disk.\r\n"));
            }
            catch (Exception ex)
            {
                this.Add(Event.CreateNewEvent(EventTypes.Error, "There was an issue saving the output to disk. The message is: " + ex + ".\r\n"));
            }
        }
    }

    /// <summary>
    /// Enumerator containing the types of allowed events (classifications).
    /// </summary>
    public enum EventTypes
    {
        Information = 0,
        Error = 1,
        Warning = 2
    }

    /// <summary>
    /// Individual event.
    /// </summary>
    public class Event
    {
        /// <summary>
        /// The classification of an event, following the EventTypes enumerator.
        /// </summary>
        public int eventCode { get; set; }

        /// <summary>
        /// Free-format description for an event.
        /// </summary>
        public string eventDescription { get; set; }

        /// <summary>
        /// Logging date/time for the event.
        /// </summary>
        public DateTime eventTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Constructor that only captures type and description of an event. This will assume 'now' as date/time of the event.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventDescription"></param>
        /// <returns></returns>
        public static Event CreateNewEvent(EventTypes eventType, string eventDescription)
        {
            var localEvent = new Event
            {
                eventCode = (int)eventType,
                eventTime = DateTime.Now,
                eventDescription = eventDescription
            };

            return localEvent;
        }

        /// <summary>
        /// Constructor that can override the date / time.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="eventDateTime"></param>
        /// <param name="eventDescription"></param>
        /// <returns></returns>
        public static Event CreateNewEvent(EventTypes eventType, DateTime eventDateTime, string eventDescription)
        {
            var localEvent = new Event
            {
                eventCode = (int)eventType,
                eventTime = eventDateTime,
                eventDescription = eventDescription
            };

            return localEvent;
        }
    }
}
