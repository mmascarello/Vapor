using System;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using VaporServer.DataAccess;

namespace VaporServer.Endpoint
{
    public class AdministrationService : Greeter.GreeterBase
    {
        private readonly ILogger<AdministrationService> _logger;
        private readonly MemoryDataBase instance;
        private readonly UserDataBase userDb;
        private readonly GameDataBase gameDB;
        private readonly AdministrationService admInstance = null;
        private static readonly object Mlock = new object();

        public AdministrationService(ILogger<AdministrationService> logger)
        {
            _logger = logger;
            this.instance = MemoryDataBase.Instance;
            this.userDb = this.instance.UserDataBase;
            this.gameDB = this.instance.GameDataBase;
        }
        

        public override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
        {
            var message = "";
            userDb.GetUsers().ForEach(x =>  message += x.UserLogin + "-");
            return Task.FromResult(new GetUsersResponse()
            {
                Message = message
            });
        }
        
        //ToDo:Refactor para que sea mantenible. 
        public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            var message = "";
            var username = request.UserName.ToLower();
            var password = request.Password.ToLower();

            try
            {
                if (!String.IsNullOrEmpty(username) &&
                    !String.IsNullOrEmpty(password))
                {
                    if (!userDb.FindUser(username)) //Todo: Refactor, nunca entra en el if.
                    {
                        var user = new User()
                        {
                            UserLogin = username,
                            Password = password
                        };
                        userDb.AddUser(user);
                        message = $"user: {username} created!";
                    }
                    else
                    {
                        message = "user already exsits";
                    }
                }
                else
                {
                    message = "user and password cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new CreateUserResponse
            {
                Message = message
            });
        }

        public override Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            var message = "";
            var username = request.UserName.ToLower();
            var newUsername = request.NewUserName.ToLower();
            var newPassword = request.NewPassword.ToLower();

            try
            {
                if (!String.IsNullOrEmpty(newUsername) &&
                    !String.IsNullOrEmpty(newPassword) &&
                    !String.IsNullOrEmpty(username))
                {
                    
                    if (userDb.FindUser(username) && !userDb.FindUser(newUsername))
                    {
                        var user = userDb.GetUser(username);

                        user.UserLogin = newUsername;
                        user.Password = newPassword;

                        userDb.ModifyUser(user);
                        message = $"user: {username} updated!";
                    }
                    else
                    {
                        message = "New user already exsits";//ToDo:Aca dejaria solo user already exists --> podriamos hacer una clase estatica con mensajes estaticos.
                    }
                }
                else
                {
                    message = "user and password cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new UpdateUserResponse()
            {
                Message = message
            });
        }
        
        public override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var message = "";
            var username = request.UserName.ToLower();

            try
            {
                if (!String.IsNullOrEmpty(username))
                {
                    if (userDb.FindUser(username))
                    {
                        userDb.DeleteUser(username);
                        message = $"user: {username} deleted!";
                    }
                    else
                    {
                        message = "user not exists";
                    }
                }
                else
                {
                    message = "username cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new DeleteUserResponse()
            {
                Message = message
            });
        }
        
        public override Task<GetGamesResponse> GetGames(GetGamesRequest request, ServerCallContext context)
        {
            var message = "";
            gameDB.GetGames().ForEach(x =>  message += x.Title+ "-");
            return Task.FromResult(new GetGamesResponse()
            {
                Message = message
            });
        }
        
        
        public override Task<CreateGameResponse> CreateGame(CreateGameRequest request, ServerCallContext context)
        {
            string message = "";
            
            var title = request.Title.ToLower();
            var gender = request.Gender.ToLower();
            var sinopsis = request.Sinopsis.ToLower();
            var ageAllowed = request.AgeAllowed;
            
            
            try
            {
                if (!String.IsNullOrEmpty(title) &&
                    !String.IsNullOrEmpty(gender) &&
                    !String.IsNullOrEmpty(sinopsis) &&
                    ageAllowed >= 0)
                {
                    if (!gameDB.FindGame(title))
                    {
                        var game = new Game()
                        {
                            Title = title,
                            Gender = gender,
                            Sinopsis = sinopsis,
                            ageAllowed = GetEsrb(ageAllowed)
                        };
                        gameDB.AddGames(game);
                        message = $"Game: {title} created!";
                    }
                    else
                    {
                        message = "Game already exsits";
                    }
                }
                else
                {
                    message = "all atributes cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new CreateGameResponse
            {
                Message = message
            });
        }

        public override Task<UpdateGameResponse> UpdateGame(UpdateGameRequest request, ServerCallContext context)
        {
            string message = "";
            
            var title = request.Title.ToLower();
            var newTitle = request.NewTitle.ToLower();
            var newgender = request.NewGender.ToLower();
            var newsinopsis = request.NewSinopsis.ToLower();
            var newageAllowed = request.NewAgeAllowed;
            
            
            try
            {
                if (!String.IsNullOrEmpty(title))
                {
                    if (gameDB.FindGame(title))
                    {
                        var game = gameDB.GetGame(title);
                        if (!String.IsNullOrEmpty(newTitle) && !gameDB.FindGame(newTitle))
                        {
                            game.Title = newTitle;
                        }
                        else
                        {
                            throw new Exception("the new title already exists");
                        }
                        if (!String.IsNullOrEmpty(newgender))
                        {
                            game.Gender = newgender;
                        }
                        if (!String.IsNullOrEmpty(newsinopsis))
                        {
                            game.Sinopsis = newsinopsis;
                        }
                        if (newageAllowed >= 0)
                        {
                            game.ageAllowed = GetEsrb(newageAllowed);
                        }
                        gameDB.ModifyGame(game);
                        message = $"Game: {title} modifed!";
                    }
                    else
                    {
                        message = "Game not found";
                    }
                }
                else
                {
                    message = "Title cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new UpdateGameResponse()
            {
                Message = message
            });
        }
        
        public override Task<DeleteGameResponse> DeleteGame(DeleteGameRequest request, ServerCallContext context)
        {
            var message = "";
            var title = request.Title.ToLower();

            try
            {
                if (!String.IsNullOrEmpty(title))
                {
                    if (gameDB.FindGame(title))
                    {
                        var game = gameDB.GetGame(title);
                        var users = userDb.GetUsers();
                        var found = users.Exists(u => u.MyOwnedGames.Exists(g => g == game.Id));
                        if (!found)
                        {
                            gameDB.DeleteGame(title);
                            message = $"game: {title} deleted!";
                        }
                        else
                        {
                            message = $"game cannot be deleted";
                        }
                    }
                    else
                    {
                        message = "Game not found";
                    }
                }
                else
                {
                    message = "game title cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new DeleteGameResponse()
            {
                Message = message
            });
        }
        
        public override Task<GetUserGamesResponse> GetUserGames(GetUserGamesRequest request, ServerCallContext context)
        {
            var message = "";
            var user = request.UserName.ToLower();
            try
            {
                if (!String.IsNullOrEmpty(user))
                {
                    if (userDb.FindUser(user))
                    {
                        var userObj = userDb.GetUser(user);
                        var games = gameDB.GetGames();
                        userObj.MyOwnedGames.ForEach(x => message+= games.Find(g => g.Id == x).Title + "-");
                    }
                    else
                    {
                        message = "user not exists";  
                    }
                   
                }else
                {
                    message = "user cannot be empty"; 
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new GetUserGamesResponse()
            {
                Message = message
            });
       }

        public override Task<BuyGameResponse> BuyGame(BuyGameRequest request, ServerCallContext context)
        {
            var message = "";
            var userName = request.UserName.ToLower();
            var title = request.Title.ToLower();

            try
            {
                if (!String.IsNullOrEmpty(title) &&
                    !String.IsNullOrEmpty(userName))
                {
                    if (gameDB.FindGame(title) && userDb.FindUser(userName))
                    {
                        var game = gameDB.GetGame(title);
                        var user = userDb.GetUser(userName);
                        if (!user.MyOwnedGames.Exists(g => g == game.Id))
                        {
                            userDb.BuyGame(user.Id,game.Id);
                            message = $"game bought!";
                        }
                        else
                        {
                            message = $"user already has this game!";
                        }
                        
                    }
                    else
                    {
                        message = "user or game not exists";
                    }
                }
                else
                {
                    message = "game title and username cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new BuyGameResponse()
            {
                Message = message
            });
        }
        
        public override Task<RefundGameResponse> RefundGame(RefundGameRequest request, ServerCallContext context)
        {
            var message = "";
            var userName = request.UserName.ToLower();
            var title = request.Title.ToLower();

            try
            {
                if (!String.IsNullOrEmpty(title) &&
                    !String.IsNullOrEmpty(userName))
                {
                    if (gameDB.FindGame(title) && userDb.FindUser(userName))
                    {
                        var game = gameDB.GetGame(title);
                        var user = userDb.GetUser(userName);
                        if (user.MyOwnedGames.Exists(g => g == game.Id))
                        {
                            userDb.RefundGame(user.Id, game.Id);
                            message = $"game refunded!";
                        }
                        else
                        {
                            message = $"user doesn't has this game!";
                        }
                    }
                    else
                    {
                        message = "user or game not exists";
                    }
                }
                else
                {
                    message = "game title and username cannot be empty";
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new RefundGameResponse()
            {
                Message = message
            });
        }
        
        
        
        
        private ESRB GetEsrb(int num)
        {
            var ageEsrb = new ESRB();
            
            switch (num)
            {
                case 0:
                    ageEsrb = ESRB.EveryOne;
                    break;
                case 1:
                    ageEsrb = ESRB.TenPlus;
                    break;
                case 2:
                    ageEsrb = ESRB.Teen;
                    break;
                case 3:
                    ageEsrb = ESRB.SeventeenPlus;
                    break;
                case 4:
                    ageEsrb = ESRB.EighteenPlus;
                    break;
                case 5:
                    ageEsrb = ESRB.Pending;
                    break;
                default:
                    ageEsrb = ESRB.EveryOne;
                    break;
            }
            return ageEsrb;
        }
        
    }
}