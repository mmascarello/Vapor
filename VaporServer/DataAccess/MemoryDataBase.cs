using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class MemoryDataBase
    {
        private readonly List<User> Users = new List<User>();
        private List<Game> Games = new List<Game>();
        
        
        public void AddUser(User user){
            this.Users.Add(user);
        }

        public List<User> GetUsers()
        {
            return Users;
        }

        public void AddGames(Game game)
        {
            this.Games.Add(game);
        }

        public List<Game> GetGames()
        {
            return Games;
        }
    }
}