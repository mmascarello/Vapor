using System;
using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class UserDataBase
    {
        private object locker = new Object();
        private List<User> users;
            
        public UserDataBase(List<User> usersDb)
        {
            this.users = usersDb; 
        }
        
        public void AddUser(User user){
            lock (locker)
            {
                this.users.Add(user);
            }
        }

        public List<User> GetUsers()
        {
            lock (locker)
            {
                return users;
            }
        }
        
        public User GetUser(string login)
        {
            lock (locker)
            {
                return users.Find(u => u.UserLogin==login);
            }
        }

        public void BuyGame(Guid userId, Guid gameId)
        {
            lock (locker)
            {
                var index = users.FindIndex(u => u.Id == userId);
                users[index].MyOwnedGames.Add(gameId);
            }
        }
    }
}