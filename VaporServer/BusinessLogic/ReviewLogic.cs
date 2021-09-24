﻿using Domain;
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

        public string GetReviewsInGame(Game game)
        {
            var reviews = reviewDataBase.GetReviews();
            var reviewsIdInGame = game.Reviews;
            var reviewsInGame ="";
            
            //ToDo: Validar que hay descripciones en el sistema. 
            
            foreach (var r in reviews)
            {
                    foreach (var rId in reviewsIdInGame)
                    {
                        if (r.Id == rId)
                        {
                            reviewsInGame += r.Description + ",";
                        }
                    }
            }
         
            return reviewsInGame;
        }

        public float RatingAverage(Game game)
        {
            var reviews = reviewDataBase.GetReviews();
            var reviewsIdInGame = game.Reviews;
            
            
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
            
            var avr = 0;
            avr = sumRating / cant;
            
            return avr;
        }
    }
}