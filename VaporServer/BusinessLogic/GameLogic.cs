using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class GameLogic
    {
        private readonly GameDataBase gameDb;
        private readonly ReviewLogic reviewLogic;
        public GameLogic(GameDataBase gameDataBase, ReviewLogic reviewLogic)
        {
            this.gameDb = gameDataBase;
            this.reviewLogic = reviewLogic;
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
                var games = gameDb.GetGames();
                
                ValidateGame(games, title);

                var receivedGame = new Game()
                    {Title = title, Gender = gender, Score = score, Sinopsis = sinopsis, CoverPage = coverPage, Id = id};
                
                gameDb.AddGames(receivedGame);
            }
            catch (Exception e)
            {
                throw new Exception("El juego ya existe, ingrese uno nuevo");
            }

        }

        private void ValidateGame(List<Game> games, string title)
        {
            var exists = games.Exists(g => g.Title.Equals(title));
            if (exists)
            {
                throw new Exception();
            }
        }

        public List<string> GetReviews(Byte[] gameTitle)
        {
            try
            {
                var game = GetGame(gameTitle);

                //obtener lista de reviews para ese juego
                var reviewsInGame = reviewLogic.GetReviewsInGame(game);
                return reviewsInGame;
            }
            catch (Exception e)
            {
                throw new Exception("El juego no existe");
            }
        }

        public string GetRatingAverage(Byte[] gameTitle)
        {
            var game = GetGame(gameTitle);
            
            var average = reviewLogic.RatingAverage(game);
            var avr = $"{average}";
            return avr ;
        }
        
        private Game GetGame(byte[] gameTitle)
        {
            var gameName = Encoding.UTF8.GetString(gameTitle);

            var games = this.gameDb.GetGames();

            var game = games.Find(g => g.Title.Equals(gameTitle));
            return game;
        }

        public List<string> GetData(Byte[] gameTitle)
        {
            //detalle de los datos del juego
            var game = GetGame(gameTitle);
            
            var data = new List<string>();
            
            data.Add(game.Gender);
            data.Add(game.Sinopsis);
            //data.Add(game.CoverPage);
            
            return data;
        }
        
        
        /*private Guid ValidateId()
        {
            var games = gameDb.GetGames();

            var invalidId = true;
            var id = new Guid();
            
            while (invalidId)
            {
                 id = Guid.NewGuid();

                invalidId = games.Exists(g => g.Id == id);
            }

            return id;
        }*/
    }
}