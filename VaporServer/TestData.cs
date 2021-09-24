using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer
{
    public static class TestData
    {
        public static void Load(GameDataBase gameDataBase, UserDataBase userDataBase, ReviewDataBase reviewDataBase)
        {
            Guid superMarioId = Guid.NewGuid();
            Guid bomermanId = Guid.NewGuid();
            Guid pokemonId = Guid.NewGuid();
            Guid warCraftId = Guid.NewGuid();

            Guid micaId = Guid.NewGuid();
            Guid maxiId = Guid.NewGuid();

            Guid reviewMica = Guid.NewGuid();
            Guid reviewMaxi = Guid.NewGuid();

            
            //ambos tienen el mismo juego y le hacen una review.
            
            reviewDataBase.AddReview(new Review()
            {
                Id = reviewMica, Description = "Este juego es muy bueno ", Rating = 4, UserId = micaId
            });
            
            reviewDataBase.AddReview(new Review()
            {
                Id = reviewMaxi, Description = "Este juego es muy malo ", Rating = 2, UserId = maxiId
            });
            
            
            gameDataBase.AddGames(new Game(){ Title = "Super Mario",Id = superMarioId}); 
            
            gameDataBase.AddGames(new Game(){ Title = "Bomberman",ageAllowed = ESRB.Teen ,Id = bomermanId, Gender = "Accion", Sinopsis = "Este es un juego para personas habilidosas", Reviews = new List<Guid>()
            {
                reviewMaxi,reviewMica
            }});
            
            gameDataBase.AddGames(new Game(){ Title = "Pokemon",Id = pokemonId});
            gameDataBase.AddGames(new Game(){ Title = "WarCraft",Id = warCraftId});
            
            userDataBase.AddUser(new User(){UserLogin = "Mica", Id = micaId, MyOwnedGames= new List<Guid>()
            {
                superMarioId,bomermanId,warCraftId
            }});
            
            userDataBase.AddUser(new User(){UserLogin = "Maxi", Id = maxiId, MyOwnedGames= new List<Guid>()
            {
                pokemonId,bomermanId
            }});
            
            


        }

    }
}
    