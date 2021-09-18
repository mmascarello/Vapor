using System;
using System.Net.Sockets;
using System.Text;
using CommunicationInterface;
using StringProtocol;
using VaporServer.BusinessLogic;
using VaporServer.Endpoint.EndpointFactory;

namespace VaporServer.Endpoint.Implementations
{
    public class GetGamesCommand : CommandInterface
    {
        
        private readonly GameLogic gameLogic;
        private readonly ICommunication communication;
        
        public GetGamesCommand(GameLogic gameLogic, ICommunication communication)
        {
            this.gameLogic = gameLogic;
            this.communication = communication;
        }


        public void Send(Socket ourSocket)
        {
            var gameList = gameLogic.GetGames();
            String games = String.Empty;
            gameList.ForEach(g => games += g.Title + "-" );
                            
            var headerToSend = new Header(HeaderConstants.Response, CommandConstants.GetGames,
                games.Length);
            communication.SendData(ourSocket,headerToSend,games);
        }

        public void Receive(Socket clientSocket, Header header)
        {
            var newBuffer = new byte[header.IDataLength];

            communication.ReceiveData(clientSocket,header.IDataLength,newBuffer);

            var data = Encoding.UTF8.GetString(newBuffer).Split('|');
        }
    }
}