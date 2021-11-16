using System;
using System.Threading.Tasks;
using AdministrationWebApi.GrpcClient;
using AdministrationWebApi.Models;
using Grpc.Net.Client;

namespace AdministratorWebApi.GrpcClient
{
    public class UserGrpc
    {
        private Greeter.GreeterClient client;
        public UserGrpc()
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:6001");
            this.client = new Greeter.GreeterClient(channel);
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