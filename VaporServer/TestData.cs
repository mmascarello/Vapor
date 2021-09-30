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
            Guid superIcyTowerId = Guid.NewGuid();

            
            Guid micaId = Guid.NewGuid();
            Guid maxiId = Guid.NewGuid();

            Guid reviewMica = Guid.NewGuid();
            Guid reviewMaxi = Guid.NewGuid();

            
            //ambos tienen el mismo juego y le hacen una review.
            
            reviewDataBase.AddReview(new Review()
            {
                Id = reviewMica, Description = "este juego es muy bueno ", Rating = 4   /*, UserId = micaId*/
            });
            
            reviewDataBase.AddReview(new Review()
            {
                Id = reviewMaxi, Description = "este juego es muy malo ", Rating = 2 /*, UserId = maxiId*/
            });
            
            
            gameDataBase.AddGames(new Game(){ Title = "super mario",Id = superMarioId, Gender = "plataforma", 
                Sinopsis = "juego mitico de plataforma", CoverPage = "super mario.png"}); 
            
            gameDataBase.AddGames(new Game(){ Title = "bomberman",ageAllowed = ESRB.Teen ,Id = bomermanId, Gender = "diversion", Sinopsis = "este es un juego para personas habilidosas", Reviews = new List<Guid>()
            {
                reviewMaxi,reviewMica
            }});
            
            gameDataBase.AddGames(new Game(){ Title = "super icy tower", ageAllowed = ESRB.SeventeenPlus ,Id =superIcyTowerId, Gender = "diversion", Sinopsis = "este es un juego para personas habilidosas"});
            
            gameDataBase.AddGames(new Game(){ Title = "pokemon",Id = pokemonId, Gender = "aventura"});
            gameDataBase.AddGames(new Game(){ Title = "warcraft",Id = warCraftId, Gender = "estrategia"});
            
            userDataBase.AddUser(new User(){UserLogin = "mica", Id = micaId, MyOwnedGames= new List<Guid>()
            {
                superMarioId,bomermanId
            }});
            
            userDataBase.AddUser(new User(){UserLogin = "maxi", Id = maxiId, MyOwnedGames= new List<Guid>()
            {
                pokemonId,bomermanId
            }});
            
        }

    }
}
    