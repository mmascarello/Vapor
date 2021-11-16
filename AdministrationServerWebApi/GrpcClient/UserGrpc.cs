using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using AdministrationServerWebApi.Models;
using Grpc.Net.Client;

namespace AdministrationServerWebApi.GrpcClient
{
    public class UserGrpc
    {
        private Greeter.GreeterClient Client;
        public UserGrpc()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            var channel = GrpcChannel.ForAddress("http://localhost:5001");
             this.Client = new Greeter.GreeterClient(channel);
        }
        public async Task<string> CreateUserAsync(UserModel userModel)
        {
            var name = userModel.Name.ToLower();
            var pw = userModel.Password.ToLower();
            var response =  await Client.CreateUserAsync(
                new CreateUserRequest{UserName = name, Password = pw});

            return response.Message;
        }

    }
}