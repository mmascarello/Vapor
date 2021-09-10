using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Common.Domain;

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
    }
}