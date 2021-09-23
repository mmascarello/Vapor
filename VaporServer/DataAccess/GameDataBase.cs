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
            lock (locker)
            {
                this.games = games;
            }
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
        
        public void NotValidGame(string title)
        {
            lock (locker)
            {
                var exists = games.Exists(g => g.Title.Equals(title));
                if (exists)
                {
                    throw new Exception();
                }
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