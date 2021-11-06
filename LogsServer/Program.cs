using System;
using System.Threading.Tasks;
using LogsServer.Factory;

namespace LogsServer
{
    class Program
    {
        private static Startup _startup = new Startup();

        static async Task Main(string[] args)
        {
            await _startup.Start().ConfigureAwait(false);
        }
    }
}