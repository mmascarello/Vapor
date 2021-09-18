using VaporCliente.Factory;


namespace VaporClient
{
    class Program
    {

        private static Startup startup = new Startup();
        static void Main(string[] args)
        {
            startup.Start();
        }

    }
}