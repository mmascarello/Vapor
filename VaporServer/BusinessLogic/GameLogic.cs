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
                var esrb = Convert.ToInt32(gameToAdd[2]);
                var sinopsis = gameToAdd[3];
                var coverPage = gameToAdd[4];
                var id = Guid.NewGuid();
               
                
                gameDb.NotValidGame(title);

                var receivedGame = new Game()
                    {Title = title, Gender = gender, Esrb = esrb, Sinopsis = sinopsis, CoverPage = coverPage, Id = id};
                
                gameDb.AddGames(receivedGame);
            }
            catch (Exception e)
            {
                throw new Exception("El juego ya existe, ingrese uno nuevo");
            }

        }

        public void ModifyGame(Byte[] game)
        {
            try
            {
                var gameToModify = Encoding.UTF8.GetString(game).Split('|');
                var gameTitle = gameToModify[0];
                var title = gameToModify[1];
                var gender = gameToModify[2];
                var esrb = gameToModify[3];
                var sinopsis = gameToModify[4];
                var coverPage = gameToModify[5];
                
                var actualGame = gameDb.GetGame(gameTitle);
                
                if (!string.IsNullOrEmpty(title))
                {
                    actualGame.Title = title;
                }
                if (!string.IsNullOrEmpty(gender))
                {
                    actualGame.Gender = gender;
                }
                if (!string.IsNullOrEmpty(esrb))
                {
                    actualGame.Esrb = Convert.ToInt32(esrb);
                }
                if (!string.IsNullOrEmpty(sinopsis))
                {
                    actualGame.Sinopsis = sinopsis;
                }
                if (!string.IsNullOrEmpty(coverPage))
                {
                    actualGame.CoverPage = coverPage;
                }
                gameDb.ModifyGame(actualGame); 
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }
        }

        public string GetCover(string game)
        {
            return gameDb.GetCover(game);
            
        }
    }
}