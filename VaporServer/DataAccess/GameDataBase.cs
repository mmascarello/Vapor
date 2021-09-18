using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class GameDataBase
    {
        private List<Game> games;
        
        public GameDataBase(List<Game> games)
        {
            this.games = games;
        }
        
        public void AddGames(Game game)
        {
            this.games.Add(game);
        }

        public List<Game> GetGames()
        {
            return games;
        }
    }
}