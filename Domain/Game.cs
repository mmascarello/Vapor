using System;
using System.Collections.Generic;
using Domain;

namespace Domain
{
    public class Game
    {
        public string Title;
        public List<Guid> Reviews;
        public int Score;
        public string Gender;
        public string Sinopsis;
        public string CoverPage;
        public Guid Id;

        public Game()
        {
            Reviews = new List<Guid>();
        }
    }
}