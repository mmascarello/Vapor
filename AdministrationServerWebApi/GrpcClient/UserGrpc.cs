using AdministrationServerWebApi.Models;

namespace AdministrationServerWebApi.GrpcClient
{
    public class UserGrpc 
    {
        private readonly GrpcConnection client;
        public UserGrpc()
        {
           client = GrpcConnection.GetGrpcConnectionInstance();
        }
        
        public string CreateUser(UserModel userModel)
        {
            var name = userModel.Name;
            var pw = userModel.Password;
            var request = new CreateUserRequest{UserName = name, Password = pw};
            var respnse = "ok";
            return respnse;
        }

    }
}