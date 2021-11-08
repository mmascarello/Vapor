using Domain;
using LogsServer.DataAccess;

namespace LogsServer.BusinessLogic
{
    public class LogLogic
    {
        private readonly LogsStorage logDb;

        public LogLogic(LogsStorage logDb)
        {
            this.logDb = logDb;
        }

        public void AddLog(Log log)
        {
            this.logDb.AddLog(log);
        }
    }
}