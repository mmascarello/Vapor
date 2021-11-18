using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.Connection;

namespace AdministratorWebApi.GrpcClient
{
    public class GameGrpc
    {
        private Greeter.GreeterClient client;
        
        public GameGrpc()
        {
            client = GrpcConnection.GetGrpcConnectionInstance().GetClient();
        }
        
        public string GetGames()
        {
            var response =  client.GetGames(
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

            return response.Message;
        }
        
        public async Task<string> DeleteGameAsync(string gameTitle)
        {
            var title = gameTitle;

            var response =  await client.DeleteGameAsync(
                new DeleteGameRequest{ Title = title});

            return response.Message;
        }
        
    }
}