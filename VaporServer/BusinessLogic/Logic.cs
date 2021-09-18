using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class Logic
    {
        public readonly GameLogic gameLogic;
        public readonly UserLogic userLogic;

        public Logic(MemoryDataBase memoryDataBase)
        {
            this.gameLogic = new GameLogic(memoryDataBase.GameDataBase);
            this.userLogic = new UserLogic(memoryDataBase.UserDataBase,gameLogic);
        }
    }
}