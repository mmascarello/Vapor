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
                var num = Convert.ToInt32(gameToAdd[2]);
                var sinopsis = gameToAdd[3];
                var coverPage = gameToAdd[4];
                var id = Guid.NewGuid();
                var games = gameDb.GetGames();
                var esbr = GetEsrb(num);

                ValidateGame(games, title);

                var receivedGame = new Game()
                {
                    Title = title, Gender = gender, ageAllowed = esbr, Sinopsis = sinopsis, CoverPage = coverPage,
                    Id = id
                };

                gameDb.AddGames(receivedGame);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

        public void DeleteGame(Byte[] game)
        {
            try
            {
                var gameToDelete = Encoding.UTF8.GetString(game);
                gameDb.DeleteGame(gameToDelete);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
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
                    actualGame.ageAllowed = GetEsrb(Convert.ToInt32(esrb));
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
//Todo: cambiar de lugar este metodo. 
        private ESRB GetEsrb(int num)
        {
            var ageEsrb = new ESRB();
            
            try
            {
                switch (num)
                {
                    case 0:
                        ageEsrb = ESRB.EveryOne;
                        break;
                    case 1:
                        ageEsrb = ESRB.TenPlus;
                        break;
                    case 2:
                        ageEsrb = ESRB.Teen;
                        break;
                    case 3:
                        ageEsrb = ESRB.SeventeenPlus;
                        break;
                    case 4:
                        ageEsrb = ESRB.EighteenPlus;
                        break;
                    case 5:
                        ageEsrb = ESRB.Pending;
                        break;
                }
            }
            catch (Exception)
            {
                throw new Exception("Ingrese un numero entre 0 y 5 para darle permisos a: \n" +
                                    "todos\n" + "mayores a 10\n" + "adolecentes\n" + "+ 17\n" + "+ 18\n " + "Pending");
            }  
            
            return ageEsrb;
        }

        private void ValidateGame(List<Game> games, string title)
        {
            var exists = games.Exists(g => g.Title.Equals(title));
            if (exists)
            {
                throw new Exception("El juego no existe");
            }
        }

        public string GetReviews(Byte[] gameTitle)
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
            var avr = "";
            avr += average;
            return avr ;
        }
        
        private Game GetGame(byte[] gameTitle)
        {
            var gameName = Encoding.UTF8.GetString(gameTitle);

            var game = this.gameDb.GetGame(gameName);

            return game;
        }

        public string GetData(Byte[] gameTitle)
        {
            //detalle de los datos del juego
            var game = GetGame(gameTitle);
            
            var data = "";
            
            data += game.Gender;
            data+="|"+game.Sinopsis;
            data+="|"+game.ageAllowed;
            
            //ToDo: Agregar la imagen
            
            return data;
        }
       
        public string GetCover(string game)
        {
            return gameDb.GetCover(game);
            
        }

        public string LookupGame(byte[] receiveGameAttributeBuffer)
        {
            var value = Encoding.UTF8.GetString(receiveGameAttributeBuffer).Split("|");
            var attribute = value[0];
            var attributeValue = value[1];
            var games = new List<Game>();
            var gamesTitles = "";
            
            Console.WriteLine(attribute+" "+attributeValue);
            
            try
            {
                if(attribute =="genero")
                {
                    Console.WriteLine("entre al if genero");

                    games = this.gameDb.GetGameByGender(attributeValue);
                    
                    Console.WriteLine("pase por la bdd");

                    gamesTitles = GetTitles(games);

                } else if (attribute.Equals("clasificacion"))
                {
                    var num = Convert.ToInt32(attributeValue);
                    var esrb = this.GetEsrb(num);
                    games = this.gameDb.GetGameByEsrb(esrb);
                    gamesTitles = GetTitles(games);
                }
                else
                {
                    games = this.gameDb.GetGamesByTitle(attributeValue);
                    gamesTitles = GetTitles(games);
                }

                return gamesTitles;
            }
            catch (Exception e)
            {
                throw new Exception("No existe un juego con ese atributo");
            }
            
        }

        private string GetTitles(List<Game> games)
        {
            var gamesToReturn = "";
            
            foreach (var g in games)
            {
                gamesToReturn += g.Title + "-";
            }

            return gamesToReturn;
        }
    }
}