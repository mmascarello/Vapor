using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer
{
    public static class TestDataGame
    {
        public static void Load(MemoryDataBase db)
        {
            db.AddGames(new Game(){ Title = "Super Mario"});
            db.AddGames(new Game(){ Title = "Bomberman"});
            db.AddGames(new Game(){ Title = "Pokemon"});
            db.AddGames(new Game(){ Title = "WarCraft"});
        }

    }
}
    