using System.Net.Sockets;
using CommunicationInterface;
using StringProtocol;
using VaporServer.BusinessLogic;
using VaporServer.Endpoint.EndpointFactory;

namespace VaporServer.Endpoint.Implementations
{
    public class BuyGameCommand: CommandInterface
    {
        private readonly GameLogic gameLogic;
        private readonly ICommunication communication;
        
        public BuyGameCommand(GameLogic gameLogic, ICommunication communication)
        {
            this.gameLogic = gameLogic;
            this.communication = communication;
        }
        
        public void Send(Socket ourSocket)
        {
            throw new System.NotImplementedException();
        }

        public void Receive(Socket clientSocket, Header header)
        {
            throw new System.NotImplementedException();
        }
    }
}