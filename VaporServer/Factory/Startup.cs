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
        

        public Startup()
        {
            this.manager = new SettingsManager();
            this.dataBase = new MemoryDataBase();
            this.businessLogic = new Logic(dataBase);
            this.server = new Server(businessLogic,manager);
        }

        public void Start()
        {
            TestDataGame.Load(dataBase);
            this.server.Start();
        }

        // factory
        // new data acces DA
        // new de business logic(DA)

        // program
        // new factory -> accede a BL -> de BL a DA


    }
}