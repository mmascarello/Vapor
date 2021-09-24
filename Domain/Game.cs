using System;
using System.Collections.Generic;
using Domain;

namespace Domain
{
    public class Game
    {
        public Guid Id;
        public string Title;
        public List<Guid> Reviews;
        public string Gender;
        public string Sinopsis;
        public string CoverPage;
        public ESRB ageAllowed;

        public Game()
        {
            Reviews = new List<Guid>();
        }
    }
}