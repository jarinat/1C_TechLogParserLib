using System;

namespace _1C_TechLogParserLib
{
    public enum EventType
    {
        Excp,
        Deadlock,
        Timeout,
        Transaction,
        Query,
        LockWait,
        Empty
    }
    public enum TransationStatuse
    {
        Empty,
        Commit,
        Rollback
    }

    public class EventDescr
    {
        public DateTime DateTime { get; set; }
        public int TimeMiliseconds { get; set; }
        public int Duration { get; set; }
        public EventType Type { get; set; }
        public short Level { get; set; }
        public int ConnID { get; set; }
        public int Conn2ID { get; set; }
        public string LockRegions { get; set; }
        public string Locks { get; set; }
        public string IBName { get; set; }
        public TransationStatuse TransactionStatus { get; set; }
        public string Context { get; set; }
        public string SQLText { get; set; }
        public string SDBLText { get; set; }
        public int Rows { get; set; }
        public int RowsAffected { get; set; }

    }
}
