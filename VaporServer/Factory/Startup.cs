using System.Threading.Tasks;
using CommunicationImplementation;
using CommunicationInterface;
using SettingsManagerImplementation;
using SettingsManagerInterface;
using VaporServer.DataAccess;
using VaporServer.BusinessLogic;
using VaporServer.Endpoint;

namespace VaporServer.Factory
{
    public class Startup
    {
        private readonly MemoryDataBase dataBase;
        private readonly Logic businessLogic;
        private readonly Server server;
        private readonly ISettingsManager manager;
        private readonly ICommunication communication;
        

        public Startup()
        {
            this.manager = new SettingsManager();
            this.dataBase = new MemoryDataBase();
            this.businessLogic = new Logic(dataBase);
            this.communication = new Communication();
            
            this.server = new Server(businessLogic,manager,communication);
        }

        public async Task Start()
        {
            TestData.Load(dataBase.GameDataBase, dataBase.UserDataBase, dataBase.ReviewDataBase);
            await this.server.Start().ConfigureAwait(false);
        }


    }
}