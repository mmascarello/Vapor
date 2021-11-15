using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using AdministrationServerWebApi.Models;
using Grpc.Net.Client;

namespace AdministrationServerWebApi.GrpcClient
{
    public class UserGrpc 
    {

        public async Task<string> CreateUserAsync(UserModel userModel)
        {
            var name = userModel.Name.ToLower();
            var pw = userModel.Password.ToLower();
            var response =  await Program.Client.CreateUserAsync(
                new CreateUserRequest{UserName = name, Password = pw});

            return response.Message;
        }

    }
}