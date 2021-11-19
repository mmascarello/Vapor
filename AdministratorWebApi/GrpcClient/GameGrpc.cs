using System;
using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.Connection;
using GrpcCommon;
using Newtonsoft.Json;

namespace AdministratorWebApi.GrpcClient
{
    public class GameGrpc
    {
        private Greeter.GreeterClient client;
        
        public GameGrpc()
        {
            client = GrpcConnection.GetGrpcConnectionInstance().GetClient();
        }
        
        public async Task<string> GetGames()
        {
            var response = await client.GetGamesAsync(
                new GetGamesRequest());
            
            return response.Message;
        }
        
        public async Task<string> CreateGameAsync(GameModel gameModel)
        {
            var title = gameModel.Title.ToLower();
            var gender = gameModel.Gender.ToLower();
            var sinopsis = gameModel.Sinopsis.ToLower();
            var ageAllowed = gameModel.AgeAllowed;
            
            var response =  await client.CreateGameAsync(
                new CreateGameRequest{ Title = title, Gender = gender, Sinopsis = sinopsis, AgeAllowed = ageAllowed});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(deserialized.Message.ToString());
            }
            
            return response.Message;
        }
        
        public async Task<string> ModifyGameAsync(string gameTitle, GameModel gameModel)
        {
            var title = gameTitle;
            var newTitle = gameModel.Title.ToLower();
            var newGender = gameModel.Gender.ToLower();
            var newSinopsis = gameModel.Sinopsis.ToLower();
            var newAgeAllowed = gameModel.AgeAllowed;
            
            var response =  await client.UpdateGameAsync(
                new UpdateGameRequest{ Title = title, NewTitle = newTitle, NewGender = newGender, NewSinopsis = newSinopsis, NewAgeAllowed = newAgeAllowed});
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(deserialized.Message.ToString());
            }
            
            return response.Message;
        }
        
        public async Task<string> DeleteGameAsync(string gameTitle)
        {
            var title = gameTitle;

            var response =  await client.DeleteGameAsync(
                new DeleteGameRequest{ Title = title});
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(deserialized.Message.ToString());
            }
            
            return response.Message;
        }
        
        public async Task<string> GetUserGamesAsync(string name)
        {
            var user = name.ToLower();
            var response = await client.GetUserGamesAsync(new GetUserGamesRequest{UserName = user});
            
            var deserialized = JsonConvert.DeserializeObject<GrpcResponse>(response.Message);
            if (deserialized.Response.Equals(Constants.Error))
            {
                throw new ArgumentException(deserialized.Message.ToString());
            }
            
            return response.Message;
        }
    }
}