using System;
using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class UserDataBase
    {
        private List<User> users;
            
        public UserDataBase(List<User> usersDb)
        {
            this.users = usersDb;
        }
        
        public void AddUser(User user){
            this.users.Add(user);
        }

        public List<User> GetUsers()
        {
            return users;
        }
        
        public void BuyGame(Guid userId , Guid gameId)
        {
            var index =  users.FindIndex(u => u.Id == userId);
            users[index].MyOwnedGames.Add(gameId);
        }
    }
}