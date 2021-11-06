using System;

namespace Domain
{
    public class Log
    {
        public string User;
        public string Game;
        public string Action;
        public string Response;
        private DateTime Date;

        public Log()
        {
            Date = DateTime.Now;
        }
    }
}