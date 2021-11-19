using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.GrpcClient;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AdministratorWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameController : ControllerBase
    {
        private GameGrpc gameGrpc;

        public GameController(GameGrpc gameGrpc)
        {
            this.gameGrpc = gameGrpc;
        }
        
        [HttpGet]
        public async Task<ActionResult<string>> GetUserGames([FromQuery(Name = "userName")] string userName)
        {
            var result = "";
            if (String.IsNullOrEmpty(userName))
            {
                result = await gameGrpc.GetGames();
            }
            else
            {
                result = await gameGrpc.GetUserGamesAsync(userName);
            }

            return result;
        }

        // POST: api/Game
        [HttpPost]
        public async Task<ActionResult<string>> PostGameAsync([FromBody] GameModel gameModel)
        {
            var result = await gameGrpc.CreateGameAsync(gameModel);
            return Created("",result);
        }

        // PUT: api/Game/amongUs
        [HttpPut("{name}")]
        public async Task<ActionResult<string>> PutGameAsync(string name, [FromBody] GameModel gameModel)
        {
            var result = await gameGrpc.ModifyGameAsync(name,gameModel);
            return result;
        }

        // DELETE: api/Game/pokemon
        [HttpDelete("{game}")]
        public async Task<ActionResult<string>> DeleteGameAsync(string game)
        {
            var result = await gameGrpc.DeleteGameAsync(game);
            return result;
       }
    }
}
