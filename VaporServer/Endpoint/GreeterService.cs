using System;
using System.Threading.Tasks;
using Domain;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using StringProtocol;
using VaporServer.DataAccess;
using VaporServer.MQHandler;

namespace VaporServer.Endpoint
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        private readonly UserDataBase userDb;
        private readonly MQProducer logsProducer;

        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
            this.userDb = MemoryDataBase.Instance.UserDataBase;
            this.logsProducer = MQProducer.Instance;
        }

        public override Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
        {
            var message = "";
            userDb.GetUsers().ForEach(x =>  message += x.UserLogin + "-");

            var response = SendLog(CommandConstants.GetGamesDescription, "", ResponseConstants.Ok);
            
            return Task.FromResult(new GetUsersResponse()
            {
                Message = message
            });
        }

        public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            var message = "";
            var username = request.UserName.ToLower();
            var password = request.Password.ToLower();
            var logResponse = "";
            try
            {
                if (!String.IsNullOrEmpty(username) &&
                    !String.IsNullOrEmpty(password))
                {
                    if (!userDb.FindUser(username))
                    {
                        var user = new User()
                        {
                            UserLogin = username,
                            Password = password
                        };
                        userDb.AddUser(user);
                        message = $"user: {username} created!";
                        logResponse = ResponseConstants.Ok;
                    }
                    else
                    {
                        message = "user already exsits";
                        logResponse = ResponseConstants.Error;
                    }
                }
                else
                {
                    message = "user and password cannot be empty";
                    logResponse = ResponseConstants.Error;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                logResponse =  ResponseConstants.Error;
            }
            
            var log = SendLog(CommandConstants.CreateUserDescription, "", logResponse);
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
            var logResponse = "";
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
                        logResponse = ResponseConstants.Ok;
                    }
                    else
                    {
                        message = "New user already exsits";
                        logResponse = ResponseConstants.Error;
                    }
                }
                else
                {
                    message = "user and password cannot be empty";
                    logResponse = ResponseConstants.Error;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                logResponse = ResponseConstants.Error;
            }
            
            var log = SendLog(CommandConstants.ModifyUserDescription, "", logResponse);
            return Task.FromResult(new UpdateUserResponse()
            {
                Message = message
            });
        }
        
        public override Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
        {
            var message = "";
            var username = request.UserName.ToLower();
            var logResponse = "";
            try
            {
                if (!String.IsNullOrEmpty(username))
                {
                    if (userDb.FindUser(username))
                    {
                        userDb.DeleteUser(username);
                        message = $"user: {username} deleted!";
                        logResponse = ResponseConstants.Ok;
                    }
                    else
                    {
                        message = "user not exists";
                        logResponse = ResponseConstants.Error;
                    }
                }
                else
                {
                    message = "username cannot be empty";
                    logResponse = ResponseConstants.Error;
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                logResponse = ResponseConstants.Error;
            }
            var log = SendLog(CommandConstants.DeleteUserDescription, "", logResponse);
            return Task.FromResult(new DeleteUserResponse()
            {
                Message = message
            });
        }
        
        private async Task SendLog(string command, string game, string response)
        {
            var log = new Log();
            log.Game = game;
            log.User = "administrative server";
            log.Action = command;
            log.Response = response;
            log.Date = DateTime.Now;
            
            await logsProducer.SendLog(log).ConfigureAwait(false);
        }
        
    }
}