using System;
using LogsServer.Factory;

namespace LogsServer
{
    class Program
    {
        private static Startup _startup = new Startup();

        public static void Main(string[] args)
        {
           _startup.Start();
           Console.ReadLine();//al crear la web api lo puedo borrar. 
        }
    }
}