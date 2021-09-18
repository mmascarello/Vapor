using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class UserLogic
    {
        private readonly UserDataBase userDb;
        private readonly GameLogic gameLogic;
        public UserLogic(UserDataBase userDataBase, GameLogic gameLogic)
        {
            this.userDb = userDataBase;
            this.gameLogic = gameLogic;
        }
        
        public List<User> GetUsers()
        {
            return this.userDb.GetUsers();
        }

        public void AddUser(string user)
        {
            //validar que no este vacio
            User userToAdd = new User();
            userToAdd.UserLogin = user;
            this.userDb.AddUser(userToAdd);
        }

        
        public void BuyGame(string user , string game)
        {
            var errorMessage = "El usuario o el juego no existen ";
            try
            {
                var users = userDb.GetUsers();
                var games = gameLogic.GetGames();
               
                //si no encuentra el usuario o juego tira excepcion. 
                var getUser = users.Find(u => u.UserLogin.Equals(user));
                var getGame = games.Find(g => g.Title.Equals(game));

                var userId = getUser.Id;
                var gameId = getGame.Id;

                if (!AlreadyBought(getUser, gameId))
                {
                    userDb.BuyGame(userId,gameId);
                }
                else
                {
                    errorMessage = "El usuario ya compró este juego ";
                    throw new Exception(errorMessage);
                }
            }
            catch (Exception e)
            {
                throw new Exception(errorMessage);
            }
        }

        private bool AlreadyBought(User getUser, Guid gameId)
        {
            return getUser.MyOwnedGames.Exists(g => g.Equals(gameId));
        }
    }
}