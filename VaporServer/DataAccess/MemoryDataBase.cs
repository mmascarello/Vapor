using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class MemoryDataBase
    {
        private readonly List<User> users = new List<User>();
        private readonly List<Game> games = new List<Game>();
        public readonly UserDataBase UserDataBase;
        public readonly GameDataBase GameDataBase;

        public MemoryDataBase()
        {
            this.UserDataBase = new UserDataBase(users);
            this.GameDataBase = new GameDataBase(games);
        }
     

      

       
    }
}