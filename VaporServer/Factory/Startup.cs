using VaporServer.DataAccess;
using VaporServer.BusinessLogic;
using VaporServer.Endpoint;

namespace VaporServer.Factory
{
    public class Startup
    {
        private readonly MemoryDataBase DataBase;
        private readonly Logic BusinessLogic;
        private readonly Server Server;

        public Startup()
        {
            this.DataBase = new MemoryDataBase();
            this.BusinessLogic = new Logic(DataBase);
            this.Server = new Server(BusinessLogic);
        }

        public void Start()
        {
            this.Server.Start();
        }

        // factory
        // new data acces DA
        // new de business logic(DA)

        // program
        // new factory -> accede a BL -> de BL a DA


    }
}