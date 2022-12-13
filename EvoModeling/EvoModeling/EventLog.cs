using System;

namespace EvoModeling
{
    public class EventLog : ICloneable
    {
        public string id;
        public bool eventSource;
        public bool threadInit;
        public int nodeId;
        public string[] accLogEntry = new string[] { };
        public int count_Create;
        public int count_Delete;
        public int count_Update;
        public int count_Total;
        public int count_AccCreate;
        public int count_AccDelete;
        public int count_AccUpdate;
        public int count_AccTotal;

        public object Clone()
        {
            EventLog clonedEventLog = new EventLog();
            clonedEventLog.id = id;
            clonedEventLog.eventSource = eventSource;
            clonedEventLog.threadInit = threadInit;
            clonedEventLog.nodeId = nodeId;
            clonedEventLog.accLogEntry = accLogEntry;
            clonedEventLog.count_Create = count_Create;
            clonedEventLog.count_Delete = count_Delete;
            clonedEventLog.count_Update = count_Update;
            clonedEventLog.count_Total = count_Total;
            clonedEventLog.count_AccCreate = count_AccCreate;
            clonedEventLog.count_AccDelete = count_AccDelete;
            clonedEventLog.count_AccUpdate = count_AccUpdate;
            clonedEventLog.count_AccTotal = count_AccTotal;
            return clonedEventLog;
        }
    }
}
