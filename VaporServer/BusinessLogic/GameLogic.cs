using System;
using System.Collections.Generic;
using System.Text;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class GameLogic
    {
        private readonly GameDataBase gameDb;

        public GameLogic(GameDataBase gameDataBase)
        {
            this.gameDb = gameDataBase;
        }

        public List<Game> GetGames()
        {
            return this.gameDb.GetGames();
        }

        public void PublicGame(Byte[] game)
        {
            try
            {
                var gameToAdd = Encoding.UTF8.GetString(game).Split('|');
                var title = gameToAdd[0];
                var gender = gameToAdd[1];
                var score = Convert.ToInt32(gameToAdd[2]);
                var sinopsis = gameToAdd[3];
                var coverPage = gameToAdd[4];
                var id = Guid.NewGuid();
               
                
                gameDb.NotValidGame(title);

                var receivedGame = new Game()
                    {Title = title, Gender = gender, Score = score, Sinopsis = sinopsis, CoverPage = coverPage, Id = id};
                
                gameDb.AddGames(receivedGame);
            }
            catch (Exception e)
            {
                throw new Exception("El juego ya existe, ingrese uno nuevo");
            }

        }

        
        public string GetCover(string game)
        {
            return gameDb.GetCover(game);
            
        }
    }
}