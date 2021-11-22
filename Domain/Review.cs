using System;

namespace Domain
{
    public class Review
    {
        
        public Guid Id;
        public string Description;
        public int Rating;
        
        public Review ()
        {
            Id = Guid.NewGuid();
        }
    }
}