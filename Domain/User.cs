﻿using System;
using System.Collections.Generic;

namespace Domain
{
    public class User
    {
        public string UserLogin;
        public List<Guid> MyOwnedGames;
        public Guid Id;

        public User()
        {
            MyOwnedGames = new List<Guid>();
        }
    }
}