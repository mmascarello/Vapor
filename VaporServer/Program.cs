using VaporServer.Factory;


namespace VaporServer
{
    class Program
    {
        private static Startup _startup = new Startup();

        static void Main(string[] args)
        {
            _startup.Start();
        }
    }
}