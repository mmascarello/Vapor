using System;
using Domain;

namespace LogWebAPI.Models
{
    public class LogModel
    {
        public string User { get; set; }
        public string Game { get; set; }
        public string Date { get; set; }

        public Log ToEntity()
        {
            return new Log()
            {
                User = this.User,
                Game = this.Game,
                Date = Convert.ToDateTime(this.Date),
            };
        }
        
    }
}