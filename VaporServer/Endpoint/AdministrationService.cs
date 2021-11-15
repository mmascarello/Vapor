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
        private readonly UserDataBase userDb;

        public AdministrationService(ILogger<AdministrationService> logger)
        {
            _logger = logger;
            this.userDb = MemoryDataBase.Instance.UserDataBase;
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
                    if (!userDb.FindUser(username))
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
                        message = "New user already exsits";
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
        
    }
}