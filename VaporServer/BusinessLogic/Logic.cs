using VaporServer.DataAccess;

namespace VaporServer.BusinessLogic
{
    public class Logic
    {
        public readonly GameLogic GameLogic;
        public readonly UserLogic UserLogic;
        public readonly ReviewLogic ReviewLogic;

        public Logic(MemoryDataBase memoryDataBase)
        {
            this.ReviewLogic = new ReviewLogic(memoryDataBase.ReviewDataBase);
            this.GameLogic = new GameLogic(memoryDataBase.GameDataBase,ReviewLogic);
            this.UserLogic = new UserLogic(memoryDataBase.UserDataBase,GameLogic);
            
        }
    }
}