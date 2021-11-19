using System;
using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.Connection;
using GrpcCommon;
using Newtonsoft.Json;

namespace AdministratorWebApi.GrpcClient
{
    public class UserGrpc
    {
        private Greeter.GreeterClient client;
        public UserGrpc()
        {
            client = GrpcConnection.GetGrpcConnectionInstance().GetClient();
        }
        
        public async Task<string> GetUsers()
        {
            var response =  await client.GetUsersAsync(
                new GetUsersRequest());
            
            return response.Message;
        }
        
        public async Task<string> GetUsersForAGameAsync(string title)
        {
            var game = title.ToLower();
            var response =  await client.GetUsersForAGameAsync(
                new GetUsersForAGameRequest{Title = game});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(response.Message);
            }
            
            return response.Message;
        }
        
        
        public async Task<string> CreateUserAsync(UserModel userModel)
        {
            var name = userModel.Name.ToLower();
            var pw = userModel.Password.ToLower();
            var response =  await client.CreateUserAsync(
                new CreateUserRequest{UserName = name, Password = pw});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(response.Message);
            }
            
            return response.Message;
        }

        public async Task<string> ModifyUserAsync(string userName, UserModel userModel)
        {
            var name = userName.ToLower();
            var newName = userModel.Name.ToLower();
            var newPw = userModel.Password.ToLower();
            
            var response =  await client.UpdateUserAsync(
                new UpdateUserRequest{UserName = name, NewUserName = newName, NewPassword = newPw });
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(response.Message);
            }
            
            return response.Message;
        }
      
        public async Task<string> DeleteUserAsync(string userName)
        {
            var name = userName.ToLower();

            var response =  await client.UpdateUserAsync(
                new UpdateUserRequest{UserName = name});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(response.Message);
            }
            
            return response.Message;
        }
        
        public async Task<string> BuyGameAsync(string userName,string gameTitle)
        {
            var user = userName;
            var title = gameTitle;

            var response =  await client.BuyGameAsync(
                new BuyGameRequest{ UserName = user,Title = title});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(response.Message);
            }
            
            return response.Message;
        }

        public async Task<string> RefundGameAsync(string userName, string gameTitle)
        {
            var user = userName;
            var title = gameTitle;

            var response =  await client.RefundGameAsync(
                new RefundGameRequest{ UserName = user,Title = title});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(response.Message);
            }
            
            return response.Message;
        }
    }
}