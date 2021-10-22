using System.Threading.Tasks;
using VaporCliente.Factory;


namespace VaporClient
{
    class Program
    {

        private static Startup startup = new Startup();
        static async Task Main(string[] args)
        {
            await startup.Start().ConfigureAwait(false);
        }

    }
}