using System.Threading.Tasks;
using LogsServer.Endpoint;

namespace LogsServer.Factory
{
    public class Startup
    {
        private readonly LogConsumer consumer;

        public Startup()
        {
            consumer = new LogConsumer();
        }

        public async Task Start()
        {
            await this.consumer.Start().ConfigureAwait(false);
        }
    }
}