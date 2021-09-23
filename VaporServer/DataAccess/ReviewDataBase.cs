using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class ReviewDataBase
    {
        private List<Review> reviews;
        
        public ReviewDataBase(List<Review> reviews)
        {
            this.reviews = reviews;
        }
  
        public void AddReview(Review review)
        {
            this.reviews.Add(review);
        }

        public List<Review> GetReviews()
        {
            return this.reviews;
        }
        

    }
}