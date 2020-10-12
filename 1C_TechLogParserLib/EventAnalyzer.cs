using System;
using System.Text.RegularExpressions;

namespace _1C_TechLogParserLib
{
    public class EventAnalyzer
    {
        #region constants
        private const string eventPattern =
            @"^\d\d:\d\d\.(\d+)-(\d+),(TTIMEOUT|TDEADLOCK|SDBL|TLOCK|DBMSSQL|DBPOSTGRS)," +
            @"(\d+),process=rphost,p:processName=([-_\w\d]+)," +
            @"(?:(?!(?:,t:connectID=))(?:.|\n))*" +
            @",t:connectID=(\d+)";

        private const string deadlockPattern = @"DeadlockConnectionIntersections=(.*)";

        private const string lockPattern = @"Regions=" +
            @"((?:(?!(?:,\w))(?:.|\n))*)," +
            @"Locks=(.)((?:(?!(?:\2,\w))(?:.|\n))*)\2," +
            @"WaitConnections=(\d+)";

        private const string longQueryPattern = @"Sql=" +
            @"((?:(?!(?:,Rows))(?:.|\n))*)," +
            @"(?:Rows=(\d+),)?(?:RowsAffected=(\d+))?";

        private const string sdblTranPattern = @"Func=Transaction,Func=(RollbackTransaction|CommitTransaction)";
        private const string sdblLongQueryPattern = @"Sdbl=" +
            @"((?:(?!(?:,Rows))(?:.|\n))*)," +
            @"(?:Rows=(\d+),)?(?:RowsAffected=(\d+))?";

        private const string timeoutPattern = @"WaitConnections=(\d+)";

        #endregion

        public EventDescr Analyze(string eventStr, DateTime eventDT)
        {
            Match m = Regex.Match(eventStr, eventPattern);
            if (!m.Success)
                return null;

            int timeMiliseconds = int.Parse(m.Groups[1].ToString());
            int eventDur = int.Parse(m.Groups[2].ToString());
            string eventName = m.Groups[3].ToString();
            short eventLevel = short.Parse(m.Groups[4].ToString());
            string eventIBName = m.Groups[5].ToString();
            int eventConnID = int.Parse(m.Groups[6].ToString());

            string eventContext = CutAndGetParamFromEndOfString(ref eventStr, ",Context=");

            int eventConn2ID = 0;
            string eventLockRegions = "";
            string eventLocks = "";
            TransationStatuse eventTransStatus = TransationStatuse.Empty;

            string eventSQLText = "";
            string eventSdblText = "";
            int eventRowsAffected = 0;
            int eventRows = 0;

            EventType evType = GetEventTypeByName(eventName);
            
            switch (evType)
            {
                case EventType.Excp:
                    break;
                case EventType.Timeout:

                    m = Regex.Match(eventStr, timeoutPattern);
                    if (m.Success)
                    {
                        eventConn2ID = int.Parse(m.Groups[1].ToString());
                    }
                    break;
                    
                case EventType.Deadlock:

                    m = Regex.Match(eventStr, deadlockPattern);
                    if (m.Success)
                    {
                        eventLocks = m.Groups[1].ToString();
                    }
                    break;
                case EventType.Transaction:
                    
                    m = Regex.Match(eventStr, sdblTranPattern);
                    if (m.Success)
                    {
                        string tStatStr = m.Groups[1].ToString();
                        eventTransStatus = tStatStr == "CommitTransaction" ? TransationStatuse.Commit : TransationStatuse.Rollback;
                    }

                    m = Regex.Match(eventStr, sdblLongQueryPattern);
                    if (m.Success)
                    {
                        eventSdblText = m.Groups[1].ToString();
                    }

                    break;

                case EventType.Query:
                    // longQueryPattern
                    m = Regex.Match(eventStr, longQueryPattern);
                    if (m.Success)
                    {
                        eventSQLText = m.Groups[1].ToString();
                        eventRows = m.Groups[2].ToString().Length > 0 ? int.Parse(m.Groups[2].ToString()) : 0;
                        eventRowsAffected = m.Groups[3].ToString().Length > 0 ? int.Parse(m.Groups[3].ToString()) : 0;
                    }
                    break;
                case EventType.LockWait:

                    m = Regex.Match(eventStr, lockPattern);
                    if (m.Success)
                    {
                        eventLockRegions = m.Groups[1].ToString();
                        eventLocks = m.Groups[3].ToString();
                        eventConn2ID = int.Parse(m.Groups[4].ToString());
                    }

                    break;
            }

            EventDescr ev = new EventDescr()
            {
                IBName = eventIBName,
                DateTime = eventDT,
                TimeMiliseconds = timeMiliseconds,
                Duration = eventDur,
                Type = evType,
                Level = eventLevel,
                ConnID = eventConnID,
                Conn2ID = eventConn2ID,
                Context = eventContext,
                LockRegions = eventLockRegions,
                Locks = eventLocks,
                TransactionStatus = eventTransStatus,
                SQLText = eventSQLText,
                SDBLText = eventSdblText,
                Rows = eventRows,
                RowsAffected = eventRowsAffected
            };
            
            return ev;
        }

        private string CutAndGetParamFromEndOfString(ref string eventStr, string paramStr)
        {
            string context = "";
            int x = eventStr.IndexOf(paramStr);
            if (x >= 0)
            {
                context = eventStr.Substring(x + paramStr.Length);
                eventStr = eventStr.Remove(x);
            }
            
            return context;
        }

        private EventType GetEventTypeByName(string eventName)
        {
            EventType evType = EventType.Empty;
            switch (eventName)
            {
                case "EXCP":
                    evType = EventType.Excp;
                    break;
                case "TTIMEOUT":
                    evType = EventType.Timeout;
                    break;
                case "TDEADLOCK":
                    evType = EventType.Deadlock;
                    break;
                case "SDBL":
                    evType = EventType.Transaction;
                    break;
                case "DBMSSQL":
                    evType = EventType.Query;
                    break;
                case "DBPOSTGRS":
                    evType = EventType.Query;
                    break;
                case "TLOCK":
                    evType = EventType.LockWait;
                    break;
            }
            return evType;
        }

    }
}
