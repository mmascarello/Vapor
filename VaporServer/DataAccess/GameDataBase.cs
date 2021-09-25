using System;
using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class GameDataBase
    {
        private List<Game> games;
        private object locker = new object();
        
        public GameDataBase(List<Game> games)
        {
            this.games = games;
        }
        
        public void AddGames(Game game)
        {
            lock (locker)
            {
                this.games.Add(game);
            }
        }

        public List<Game> GetGames()
        {
            lock (locker)
            {
                return games;
            }
        }

        public Game GetGame(string title)
        {
            var game=new Game();
            
            lock (locker)
            {
                game = games.Find(g => g.Title.Equals(title));
            }

            return game;
        }
        
        public void ModifyGame(Game gameModified)
        {
            lock (locker)
            {
                var index = games.FindIndex(g => g.Id == gameModified.Id);
                games[index] = gameModified;

            }
        }

        public string GetCover(string game)
        {
            try
            {
                lock (locker)
                {
                    var gameObject = games.Find(g => g.Title.Equals(game));
                    return gameObject.CoverPage;
                }
            }
            catch (Exception e)
            {
                throw new Exception("El juego no existe");
            }
        }
    }
}