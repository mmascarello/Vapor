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
        
        public void ModifyUser(User userModified)
        {
            try
            {
                lock (locker)
                {
                    var index = users.FindIndex(u => u.Id == userModified.Id);
                    users[index] = userModified;

                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al actualizar usuario");
            }
            
        }
        
        public void DeleteUser(string user)
        {
            try
            {
                lock (locker)
                {
                    var userToRemove = GetUser(user);
                    users.Remove(userToRemove);
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public List<User> GetUsers()
        {
            lock (locker)
            {
                return users;
            }
        }
        
        public bool FindUser(string userName)
        {
            lock (locker)
            {
                return  users.Exists(u => u.UserLogin == userName);
            }
        }
        
        public User GetUser(string userName)
        {
            try
            {
                lock (locker)
                {
                    var user = users.Find(u => u.UserLogin == userName);
                    return user;
                }
            }
            catch (Exception e)
            {
                throw new Exception("No user found");
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

        public User Login(string user, string password)
        {
            lock (locker)
            {
                var findUser = users.Find(u => u.UserLogin.Equals(user) && u.Password.Equals(password));
                return findUser;
            }
        }
    }
}