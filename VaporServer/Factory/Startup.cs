using System;
using System.Threading.Tasks;
using CommunicationImplementation;
using CommunicationInterface;
using SettingsManagerImplementation;
using SettingsManagerInterface;
using VaporServer.DataAccess;
using VaporServer.BusinessLogic;
using VaporServer.Endpoint;
using VaporServer.MQHandler;

namespace VaporServer.Factory
{
    public class Startup
    {
        private readonly MemoryDataBase dataBase;
        private readonly Logic businessLogic;
        private readonly Server server;
        private readonly ISettingsManager manager;
        private readonly ICommunication communication;
        private readonly MQProducer logsProducer;
        

        public Startup()
        {
            this.manager = new SettingsManager();
            this.dataBase = MemoryDataBase.Instance;
            this.businessLogic = new Logic(dataBase);
            this.communication = new Communication();

            this.server = new Server(businessLogic,manager,communication);
        }

        public async Task Start()
        {   
            Console.WriteLine("Iniciado.");
            TestData.Load(dataBase.GameDataBase, dataBase.UserDataBase, dataBase.ReviewDataBase);
            await this.server.Start().ConfigureAwait(false);
        }
        
        
    }
}