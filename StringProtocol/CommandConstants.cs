using System;

namespace StringProtocol
{
    public static class CommandConstants
    {
        public const int GetGames = 1;
        public const string  GetGamesDescription = "GetGames";
        
        public const int BuyGame = 2;
        public const string  BuyGameDescription = "BuyGame";
        
        public const int SendImage = 3;
        public const string  SendImageDescription = "SendImage";
        
        public const int PublicGame = 4;
        public const string  PublicGameDescription = "PublicGame";

        public const int GameDetail = 5;
        public const string  GameDetailDescription = "GameDetail";

        public const int  ModifyGame= 6;
        public const string  ModifyGameDescription = "ModifyGame";

        public const int LookupGame = 7;
        public const string  LookupGameDescription = "LookupGame";
        
        public const int  DeleteGame= 8;
        public const string  DeleteGameDescription = "DeleteGame";

        public const int  PublicCalification= 9;
        public const string  PublicCalificationDescription = "PublicCalification";

        public const int  Login= 10;
        public const string  LoginDescription = "Login";
        
        public const string  ModifyUserDescription = "ModifyUser";


        public const string CreateUserDescription = "CreateUser";
        public const string DeleteUserDescription = "DeleteUser";
    }
}