using System;

namespace Domain
{
    public class Log
    {
        public int Id { get; set; }
        public string User { get; set; }
        public string Game { get; set; }
        public string Action { get; set; }
        public string Response { get; set; }
        public DateTime Date {  get;  set; }
    }
    
    
}