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
        
        public async Task<string> CreateUserAsync(UserModel userModel)
        {
            var name = userModel.Name.ToLower();
            var pw = userModel.Password.ToLower();
            var response =  await client.CreateUserAsync(
                new CreateUserRequest{UserName = name, Password = pw});

            return response.Message;
            
        }

        public string GetUsers()
        {
            var response =  client.GetUsers(
                new GetUsersRequest());
            
            return response.Message;
        }

    }
}