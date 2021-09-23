using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer
{
    public static class TestData
    {
        public static void Load(GameDataBase gameDataBase, UserDataBase userDataBase)
        {
            Guid superMarioId = Guid.NewGuid();
            Guid bomermanId = Guid.NewGuid();
            Guid pokemonId = Guid.NewGuid();
            Guid warCraftId = Guid.NewGuid();

            Guid micaId = Guid.NewGuid();
            Guid maxiId = Guid.NewGuid();

            gameDataBase.AddGames(new Game(){ Title = "Super Mario",Id = superMarioId, CoverPage = "Super Mario.png"}); 
            gameDataBase.AddGames(new Game(){ Title = "Bomberman",Id = bomermanId});
            gameDataBase.AddGames(new Game(){ Title = "Pokemon",Id = pokemonId});
            gameDataBase.AddGames(new Game(){ Title = "WarCraft",Id = warCraftId});
            
            userDataBase.AddUser(new User(){UserLogin = "Mica", Id = micaId, MyOwnedGames= new List<Guid>()
            {
                superMarioId,bomermanId
            }});
            
            userDataBase.AddUser(new User(){UserLogin = "Maxi", Id = maxiId, MyOwnedGames= new List<Guid>()
            {
                pokemonId,warCraftId
            }});
            
        }

    }
}
    