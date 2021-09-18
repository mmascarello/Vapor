using System.Net.Sockets;
using CommunicationInterface;
using StringProtocol;
using VaporServer.BusinessLogic;
using VaporServer.Endpoint.Implementations;

namespace VaporServer.Endpoint.EndpointFactory
{
    public abstract class CommandFactory
    {
       public static CommandInterface CreateServerCommand(Logic businessLogic, ICommunication communication, int command)
        {
            if (command == CommandConstants.BuyGame)
            {
                return new BuyGameCommand(businessLogic.gameLogic, communication);
            }
            else
            {
                return new GetGamesCommand(businessLogic.gameLogic, communication);
            }
        }

    }
}