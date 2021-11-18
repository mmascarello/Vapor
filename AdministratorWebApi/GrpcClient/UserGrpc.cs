using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.Connection;

namespace AdministratorWebApi.GrpcClient
{
    public class UserGrpc
    {
        private Greeter.GreeterClient client;
        public UserGrpc()
        {
            client = GrpcConnection.GetGrpcConnectionInstance().GetClient();
        }
        
        public string GetUsers()
        {
            var response =  client.GetUsers(
                new GetUsersRequest());
            
            return response.Message;
        }
        
        public async Task<string> CreateUserAsync(UserModel userModel)
        {
            var name = userModel.Name.ToLower();
            var pw = userModel.Password.ToLower();
            var response =  await client.CreateUserAsync(
                new CreateUserRequest{UserName = name, Password = pw});

            return response.Message;
        }

        public async Task<string> ModifyUserAsync(string userName, UserModel userModel)
        {
            var name = userName.ToLower();
            var newName = userModel.Name.ToLower();
            var newPW = userModel.Password.ToLower();
            
            var response =  await client.UpdateUserAsync(
                new UpdateUserRequest{UserName = name, NewUserName = newName, NewPassword = newPW });

            return response.Message;
        }
      
        public async Task<string> DeleteUserAsync(string userName)
        {
            var name = userName.ToLower();

            var response =  await client.UpdateUserAsync(
                new UpdateUserRequest{UserName = name});

            return response.Message;
        }
        
        public async Task<string> BuyGameAsync(string userName,string gameTitle)
        {
            var user = userName;
            var title = gameTitle;

            var response =  await client.BuyGameAsync(
                new BuyGameRequest{ UserName = user,Title = title});

            return response.Message;
        }

        public async Task<string> RefundGameAsync(string userName, string gameTitle)
        {
            var user = userName;
            var title = gameTitle;

            var response =  await client.RefundGameAsync(
                new RefundGameRequest{ UserName = user,Title = title});

            return response.Message;
        }
    }
}