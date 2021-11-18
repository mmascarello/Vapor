using System;
using System.Text;
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
        
        public string GetUsers()
        {
            Console.WriteLine("Estoy en get user GRPC");
            try
            {
                var users = this.userDb.GetUsers();
                var usersToShow = "";
                users.ForEach(u=> usersToShow+= u.UserLogin +"-");
                return usersToShow;

            }
            catch (Exception e)
            {
                throw new Exception("No hay usuarios en el sistema");
            }
        }

        public void CreateUser(string user, string password)
        {
            User userToAdd = new User();
            userToAdd.UserLogin = user;
            userToAdd.Password = password;
            if (!ExistsUser(user))
            {
                this.userDb.AddUser(userToAdd);
            }
            else
            {
                throw new Exception("El usuario ya existe");
            }
            
            
            
        }
        
        public void ModifyUser(string userName,string newName, string password)
        {
            try
            {
                var user = userDb.GetUser(userName);

                if (!string.IsNullOrEmpty(newName))
                {
                    user.UserLogin = newName;
                }
                
                if (!string.IsNullOrEmpty(password))
                {
                    user.Password = password;
                }

                this.userDb.ModifyUser(user);

            }
            catch (Exception e)
            {
                Console.WriteLine("El usuario a modificar no existe");
            }
        
        }
        
        //ToDo: mejorar metodo. 
        public bool ExistsUser(string user)
        {
            var exists = false;

            try
            {
                var foundUser = userDb.GetUser(user);
                
                if (foundUser.UserLogin == user)
                {
                    exists = true;
                }

                return exists;
            }
            catch (Exception e)
            {
               return exists;
            }
          
        }
        public void DeleteUser(string user)
        {
            try
            {
                this.userDb.DeleteUser(user);
            }
            catch (Exception e)
            {
                Console.WriteLine("User not found");
            }
        }

        
        public void DeleteGame(byte[] game)
        {
            try
            {
                var gameToDelete = Encoding.UTF8.GetString(game);
                var games = gameLogic.GetGames();
                var gameFound = games.Exists(g => g.Title == gameToDelete);
                if (gameFound)
                {
                    var gameObject = games.Find(g => g.Title == gameToDelete);
                    var users = userDb.GetUsers();
                
                    var found = users.Exists(u => u.MyOwnedGames.Exists(g => g == gameObject.Id));
                    if (!found)
                    {
                        gameLogic.RemoveGame(gameToDelete);
                    }else
                    {
                        throw new Exception("Game cannot be deleted");
                    }
                }
                else
                {
                    throw new Exception("Game not exists");
                }
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }
        
        
        public void BuyGame(string user , string game)
        {
            var errorMessage = "El usuario o el juego no existen";
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
                    errorMessage = "El usuario ya compro este juego";
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

        public string UserDetail(string user)
        {
            try
            {
                var userFound = userDb.GetUser(user);
                var userInfo = userFound.UserLogin + '|' + userFound.Password + '|';

                var userGamesId = userFound.MyOwnedGames;
                var games = gameLogic.GetGames();

                var userGames = "";

                foreach (var gameId in userGamesId)
                {
                    foreach (var game in games)
                    {
                        if (gameId == game.Id)
                        {
                            userGames += game.Title + ',';
                        }
                    }
                }

                userInfo += userGames;

                return userInfo;
            }
            catch (Exception e)
            {
                Console.WriteLine("No existe el usuario");
                throw;
            }
        }

        public User Login(Byte[] data)
        {
            var userName = Encoding.UTF8.GetString(data).Split('|');
            try
            {
               var user = userDb.Login(userName[0], userName[1]);
               return user; 
            }
            catch (Exception)
            {
                throw new Exception("no se puede loguear");
            }
            
        }
        
    }
}