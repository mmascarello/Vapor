using System;
using System.Collections.Generic;

namespace Domain
{
    public class User
    {
        public string UserLogin;
        public string Password;
        public List<Guid> MyOwnedGames;
        public Guid Id;

        public User()
        {
            MyOwnedGames = new List<Guid>();
            Id = Guid.NewGuid();
        }
    }
}