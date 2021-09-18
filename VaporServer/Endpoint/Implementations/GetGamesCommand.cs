using System;
using System.Net.Sockets;
using EndpointFactory;
using StringProtocol;
using VaporServer.BusinessLogic;

namespace EndpointFactory.Implementations
{
    public class GetGamesCommand : CommandInterface
    {
        private readonly Socket socket;
        private readonly GameLogic gameLogic;
        public GetGamesCommand(Socket socket,GameLogic gameLogic)
        {
            this.socket = socket;
            this.gameLogic = gameLogic;
        }
        
        public void Send()
        {
            var gameList = gameLogic.GetGames();
            String games = String.Empty;
            gameList.ForEach(g => games += g.Title + "-" );
                            
            var headerToSend = new Header(HeaderConstants.Response, CommandConstants.GetGames,
                games.Length);
            communication.SendData(clientSocket,headerToSend,games);
        }

        public void Receive()
        {
            throw new System.NotImplementedException();
        }
    }
}