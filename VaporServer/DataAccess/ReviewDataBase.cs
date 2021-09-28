using System.Collections.Generic;
using Domain;

namespace VaporServer.DataAccess
{
    public class ReviewDataBase
    {
        private List<Review> reviews;
        private readonly object locker = new object();
        
        public ReviewDataBase(List<Review> reviews)
        {
            this.reviews = reviews;
        }
  
        public void AddReview(Review review)
        {
            lock (locker)
            { 
                this.reviews.Add(review); 
            }
            
        }

        public List<Review> GetReviews()
        {
            lock (locker)
            {
                return this.reviews;
            }
        }


    }
}