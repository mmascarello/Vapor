using System.Collections.Generic;
using Domain;

namespace LogsServer.DataAccess
{
    public class LogsStorage
    {
        private readonly List<Log> logs = new List<Log>();
        private object locker = new object();

        public void AddLog(Log log)
        {
            lock (locker)
            {
                this.logs.Add(log);
            } 
        }
    }
}