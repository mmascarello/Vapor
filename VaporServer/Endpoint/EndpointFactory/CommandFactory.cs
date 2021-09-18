using System.Net.Sockets;
using CommunicationInterface;
using EndpointFactory.Implementations;
using StringProtocol;
using VaporServer.BusinessLogic;

namespace EndpointFactory
{
    public  abstract class CommandFactory
    {
        private readonly ICommunication communication;
        private readonly Logic businessLogic;
        
        public static CommandInterface CreateServerCommand(int command,Socket socket,Logic businessLogic)
        {
            if (command == CommandConstants.BuyGame)
            {
                return new BuyGameCommand();
            }
            else
            {
                return new GetGamesCommand(socket,businessLogic.gameLogic);
            }
        }

    }
}