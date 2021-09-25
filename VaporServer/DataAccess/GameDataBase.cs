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
            Game game;
            try
            {
                lock (locker)
                {
                     game = games.Find(g => g.Title.Equals(title));
                }
            }
            catch (ArgumentNullException e)
            {
                throw new Exception("El juego no existe");
            }
            return game;
            
        }
        
        public void ModifyGame(Game gameModified)
        {
            try
            {
                lock (locker)
                {
                    var index = games.FindIndex(g => g.Id == gameModified.Id);
                    games[index] = gameModified;

                }
            }
            catch (Exception e)
            {
                throw new Exception("Error al actualizar el juego");
            }
            
        }

        public string GetCover(string game)
        {
            try
            {
                lock (locker)
                {
                    var gameObject = GetGame(game);
                    return gameObject.CoverPage;
                }
            }
            catch (Exception e)
            {
                throw new Exception("No existe el juego");
            }
        }

        public void DeleteGame(string game)
        {
            try
            {
                lock (locker)
                {
                    var gameToRemove = GetGame(game);
                    games.Remove(gameToRemove);
                }
            }catch (Exception e)
            {
                throw new Exception(e.Message);
            }

        }

    }
}