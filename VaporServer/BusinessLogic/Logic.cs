using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class Logic
    {
        private readonly MemoryDataBase _memoryDataBase;

        public Logic(MemoryDataBase memoryDataBase)
        {
            this._memoryDataBase = memoryDataBase;
        }

        public List<User> GetUsers()
        {
            return this._memoryDataBase.GetUsers();
        }

        public void AddUser(string user)
        {
            //validar que no este vacio
            User userToAdd = new User();
            userToAdd.UserLogin = user;
            this._memoryDataBase.AddUser(userToAdd);
        }
        
        public List<Game> GetGames()
        {
            return this._memoryDataBase.GetGames();
        }
    }
}