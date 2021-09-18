using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer
{
    public static class TestData
    {
        public static void Load(MemoryDataBase db)
        {
            Guid superMarioId = new Guid();
            Guid bomermanId = new Guid();
            Guid pokemonId = new Guid();
            Guid warCraftId = new Guid();

            Guid micaId = new Guid();
            Guid maxiId = new Guid();

            db.AddGames(new Game(){ Title = "Super Mario",Id = superMarioId});
            db.AddGames(new Game(){ Title = "Bomberman",Id = bomermanId});
            db.AddGames(new Game(){ Title = "Pokemon",Id = pokemonId});
            db.AddGames(new Game(){ Title = "WarCraft",Id = warCraftId});
            
            db.AddUser(new User(){UserLogin = "Mica", Id = micaId, MyOwnedGames= new List<Guid>()
            {
                superMarioId,bomermanId
            }});
            
            db.AddUser(new User(){UserLogin = "Maxi", Id = maxiId, MyOwnedGames= new List<Guid>()
            {
                pokemonId,warCraftId
            }});
            
        }

    }
}
    