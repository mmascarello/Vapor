using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class GameLogic
    {
        private readonly GameDataBase gameDb;

        public GameLogic(GameDataBase gameDataBase)
        {
            this.gameDb = gameDataBase;
        }

        public List<Game> GetGames()
        {
            return this.gameDb.GetGames();
        }
    }
}