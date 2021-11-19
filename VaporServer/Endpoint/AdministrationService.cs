using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VaporServer.DataAccess;
using GrpcCommon;



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
            this._logger = logger;
            this.instance = MemoryDataBase.Instance;
            this.userDb = this.instance.UserDataBase;
            this.gameDB = this.instance.GameDataBase;
        }
        

        public override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
        {
            
            var grpcResponse = new GrpcResponse();
            grpcResponse.Response = Constants.Ok;
            grpcResponse.Message = userDb.GetUsers();
            
            return Task.FromResult(new GetUsersResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<GetUsersForAGameResponse> GetUsersForAGame(GetUsersForAGameRequest request, ServerCallContext context)
        {
            var title = request.Title;
            var grpcResponse = new GrpcResponse();
            if (!string.IsNullOrEmpty(title) && gameDB.FindGame(title))
            {
                var game = gameDB.GetGame(title);
                var users = userDb.GetUsers().FindAll(u => u.MyOwnedGames.Exists(g=>g==game.Id));
                grpcResponse.Response = Constants.Ok;
                grpcResponse.Message = users;
            }
            else
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = "game not found";
            }
            return Task.FromResult(new GetUsersForAGameResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            var username = request.UserName.ToLower();
            var password = request.Password.ToLower();
            var grpcResponse = new GrpcResponse();
            
            try
            {
                if (!string.IsNullOrEmpty(username) &&
                    !string.IsNullOrEmpty(password))
                {
                    if (!userDb.FindUser(username)) //Todo: Refactor, nunca entra en el if.
                    {
                        User user = new User()
                        {
                            UserLogin = username,
                            Password = password
                        };
                        userDb.AddUser(user);
                        grpcResponse.Response = Constants.Ok;
                        grpcResponse.Message = user;
                        
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "the user exists";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "user and Password cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new CreateUserResponse
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }

        public override Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            var username = request.UserName.ToLower();
            var newUsername = request.NewUserName.ToLower();
            var newPassword = request.NewPassword.ToLower();

            try
            {
                if (!string.IsNullOrEmpty(newUsername) &&
                    !string.IsNullOrEmpty(newPassword) &&
                    !string.IsNullOrEmpty(username))
                {
                    
                    if (userDb.FindUser(username) && (!userDb.FindUser(newUsername) || username == newUsername ))
                    {
                        var user = userDb.GetUser(username);

                        user.UserLogin = newUsername;
                        user.Password = newPassword;

                        userDb.ModifyUser(user);
                        grpcResponse.Response = Constants.Ok;
                        grpcResponse.Message = user;
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "new user already exsits";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "user and password cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new UpdateUserResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            var username = request.UserName.ToLower();

            try
            {
                if (!string.IsNullOrEmpty(username))
                {
                    if (userDb.FindUser(username))
                    {
                        userDb.DeleteUser(username);
                        grpcResponse.Response=Constants.Ok;
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "user not exists";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message ="username cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new DeleteUserResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<GetGamesResponse> GetGames(GetGamesRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            grpcResponse.Response = Constants.Ok;
            grpcResponse.Message = gameDB.GetGames();
            
            return Task.FromResult(new GetGamesResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        
        public override Task<CreateGameResponse> CreateGame(CreateGameRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            
            var title = request.Title.ToLower();
            var gender = request.Gender.ToLower();
            var sinopsis = request.Sinopsis.ToLower();
            var ageAllowed = request.AgeAllowed;
            
            
            try
            {
                if (!string.IsNullOrEmpty(title) &&
                    !string.IsNullOrEmpty(gender) &&
                    !string.IsNullOrEmpty(sinopsis) &&
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
                        grpcResponse.Response = Constants.Ok;
                        grpcResponse.Message = game;
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "game already exsits";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "all atributes cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new CreateGameResponse
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }

        public override Task<UpdateGameResponse> UpdateGame(UpdateGameRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            
            var title = request.Title.ToLower();
            var newTitle = request.NewTitle.ToLower();
            var newgender = request.NewGender.ToLower();
            var newsinopsis = request.NewSinopsis.ToLower();
            var newageAllowed = request.NewAgeAllowed;
            
            
            try
            {
                if (!string.IsNullOrEmpty(title))
                {
                    if (gameDB.FindGame(title))
                    {
                        var game = gameDB.GetGame(title);
                        if (!string.IsNullOrEmpty(newTitle) && (!gameDB.FindGame(newTitle) || newTitle == title))
                        {
                            game.Title = newTitle;
                        }
                        else
                        {
                            throw new Exception("the new title already exists");
                        }
                        if (!string.IsNullOrEmpty(newgender))
                        {
                            game.Gender = newgender;
                        }
                        if (!string.IsNullOrEmpty(newsinopsis))
                        {
                            game.Sinopsis = newsinopsis;
                        }
                        if (newageAllowed >= 0)
                        {
                            game.ageAllowed = GetEsrb(newageAllowed);
                        }
                        gameDB.ModifyGame(game);
                        grpcResponse.Response = Constants.Ok;
                        grpcResponse.Message = game;
                        
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "game not found";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "title cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new UpdateGameResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<DeleteGameResponse> DeleteGame(DeleteGameRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            var title = request.Title.ToLower();

            try
            {
                if (!string.IsNullOrEmpty(title))
                {
                    if (gameDB.FindGame(title))
                    {
                        var game = gameDB.GetGame(title);
                        var users = userDb.GetUsers();
                        var found = users.Exists(u => u.MyOwnedGames.Exists(g => g == game.Id));
                        if (!found)
                        {
                            gameDB.DeleteGame(title);
                            grpcResponse.Response = Constants.Ok;
                        }
                        else
                        {
                            grpcResponse.Response = Constants.Error;
                            grpcResponse.Message = "the game cannot be deleted, a user purchased this game.";
                        }
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "game not found";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "game title cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new DeleteGameResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<GetUserGamesResponse> GetUserGames(GetUserGamesRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            var user = request.UserName.ToLower();
            try
            {
                if (!string.IsNullOrEmpty(user))
                {
                    if (userDb.FindUser(user))
                    {
                        var userObj = userDb.GetUser(user);
                        var games = gameDB.GetGames();
                        List<Game> gamesToReturn = new List<Game>();
                        userObj.MyOwnedGames.ForEach(x => gamesToReturn.Add(games.Find(g => g.Id == x)));
                        grpcResponse.Response = Constants.Ok;
                        grpcResponse.Message = gamesToReturn;

                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "user not exists";  
                    }
                   
                }else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "user cannot be empty"; 
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new GetUserGamesResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
       }

        public override Task<BuyGameResponse> BuyGame(BuyGameRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            var userName = request.UserName.ToLower();
            var title = request.Title.ToLower();

            try
            {
                if (!string.IsNullOrEmpty(title) &&
                    !string.IsNullOrEmpty(userName))
                {
                    if (gameDB.FindGame(title) && userDb.FindUser(userName))
                    {
                        var game = gameDB.GetGame(title);
                        var user = userDb.GetUser(userName);
                        if (!user.MyOwnedGames.Exists(g => g == game.Id))
                        {
                            userDb.BuyGame(user.Id,game.Id);
                            grpcResponse.Response = Constants.Ok;
                            grpcResponse.Message = "game bought!";
                        }
                        else
                        {
                            grpcResponse.Response = Constants.Error;
                            grpcResponse.Message ="user already has this game!";
                        }
                        
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message = "user or game not exists";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "game title and username cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new BuyGameResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
            });
        }
        
        public override Task<RefundGameResponse> RefundGame(RefundGameRequest request, ServerCallContext context)
        {
            var grpcResponse = new GrpcResponse();
            var userName = request.UserName.ToLower();
            var title = request.Title.ToLower();

            try
            {
                if (!string.IsNullOrEmpty(title) &&
                    !string.IsNullOrEmpty(userName))
                {
                    if (gameDB.FindGame(title) && userDb.FindUser(userName))
                    {
                        var game = gameDB.GetGame(title);
                        var user = userDb.GetUser(userName);
                        if (user.MyOwnedGames.Exists(g => g == game.Id))
                        {
                            userDb.RefundGame(user.Id, game.Id);
                            
                            grpcResponse.Response = Constants.Ok;
                            grpcResponse.Message ="game refunded!";
                        }
                        else
                        {
                            grpcResponse.Response = Constants.Error;
                            grpcResponse.Message ="user doesn't have this game!";
                        }
                    }
                    else
                    {
                        grpcResponse.Response = Constants.Error;
                        grpcResponse.Message ="user or game not exists";
                    }
                }
                else
                {
                    grpcResponse.Response = Constants.Error;
                    grpcResponse.Message = "game title and username cannot be empty";
                }
            }
            catch (Exception e)
            {
                grpcResponse.Response = Constants.Error;
                grpcResponse.Message = e.Message;
            }

            return Task.FromResult(new RefundGameResponse()
            {
                Message = JsonConvert.SerializeObject(grpcResponse)
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