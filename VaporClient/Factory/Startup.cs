using CommunicationImplementation;
using CommunicationInterface;
using SettingsManagerImplementation;
using SettingsManagerInterface;
using VaporCliente.Endpoint;

namespace VaporCliente.Factory
{
    public class Startup
    {
        
        private readonly ISettingsManager manager;
        private readonly ICommunication communication;
        private readonly Client client;

        public Startup()
        {
            this.manager = new SettingsManager();
            this.communication = new Communication();
            this.client = new Client(communication, manager);
        }

        public void Start()
        {
            this.client.Start();
        }
    }
}