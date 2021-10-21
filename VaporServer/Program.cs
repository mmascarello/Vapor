using System.Threading.Tasks;
using VaporServer.Factory;


namespace VaporServer
{
    class Program
    {
        private static Startup _startup = new Startup();

        static async Task Main(string[] args)
        {
            await _startup.Start();
        }
    }
}