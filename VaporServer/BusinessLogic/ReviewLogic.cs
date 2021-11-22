using System;
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

        public string GetReviewsInGame(Game game)
        {
            var reviews = reviewDataBase.GetReviews();
            var reviewsIdInGame = game.Reviews;
            var reviewsInGame ="";
            
             
            
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
            
            Console.WriteLine("antes de recorrer las listas");
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
            Console.WriteLine("antes de recorrer las listas");
            
            var avr = 0;
            if (cant > 0)
            {
                avr = sumRating / cant;
            }
            
            return avr;
        }
        
        public Guid PublicReviewInGame(string description, int rating)
        {
            var review = new Review();

            var reviewId = Guid.NewGuid();

            review.Description = description;
            review.Rating = rating;
            review.Id = reviewId;
            
            reviewDataBase.AddReview(review);
            
            return reviewId;
        }
    }
}