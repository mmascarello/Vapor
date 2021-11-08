using System.Threading.Tasks;
using LogsServer.BusinessLogic;
using LogsServer.DataAccess;
using LogsServer.Endpoint;

namespace LogsServer.Factory
{
    public class Startup
    {
        private readonly LogLogic logLogic;
        private readonly LogsStorage logDb;
        private readonly LogConsumer logConsumer;

        public Startup()
        {
            this.logDb = new LogsStorage();
            this.logLogic = new LogLogic(logDb);
            this.logConsumer = new LogConsumer(logLogic);
        }

        public void Start()
        {
            this.logConsumer.ReceiveLogs();
        }
    }
}