using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class MemoryDataBase
    {
        private readonly List<User> users = new List<User>();
        private readonly List<Game> games = new List<Game>();
        private readonly List<Review> reviews = new List<Review>();

        public readonly UserDataBase UserDataBase;
        public readonly GameDataBase GameDataBase;
        public readonly ReviewDataBase ReviewDataBase;

        private static MemoryDataBase instance = null;
        private static readonly object Mlock = new object();
        
        public MemoryDataBase()
        {
           UserDataBase = new UserDataBase(users);
           GameDataBase = new GameDataBase(games);
           ReviewDataBase = new ReviewDataBase(reviews); 
        }
        
        public static MemoryDataBase Instance
        {
            get
            {
                lock (Mlock)
                {
                    return instance ??= new MemoryDataBase();
                }
            }
        }
    }
}