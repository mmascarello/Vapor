using System;
using System.Collections.Generic;
using Domain;
using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class ReviewLogic
    {
        private readonly ReviewDataBase reviewDataBase;
        
        public ReviewLogic(ReviewDataBase reviewDataBase)
        {
            this.reviewDataBase = reviewDataBase;
        }

        public List<string> GetReviewsInGame(Game game)
        {
            var reviews = reviewDataBase.GetReviews();
            var reviewsIdInGame = game.Reviews;
            var reviewsInGame = new List<string>();
            
            //ToDo: Validar que hay descripciones en el sistema. 
            
                foreach (var r in reviews)
                {
                    foreach (var rId in reviewsIdInGame)
                    {
                        if (r.Id == rId)
                        {
                            reviewsInGame.Add(r.Description);
                        }

                    }
                }
         
            return reviewsInGame;
        }

        public float RatingAverage(Game game)
        {
            var reviews = reviewDataBase.GetReviews();
            var reviewsIdInGame = game.Reviews;
            var avr = 0;
            var cant = 0;
            var sumRating = 0;
            
            
            foreach (var r in reviews)
            {
                foreach (var rId in reviewsIdInGame)
                {
                    if (r.Id == rId)
                    {
                        cant++;
                        sumRating += r.Rating;
                    }

                }
            }

            avr = sumRating / cant;
            
            return avr;
        }
    }
}