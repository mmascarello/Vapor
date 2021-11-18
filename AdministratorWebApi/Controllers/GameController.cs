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
        
        
        // GET: api/Game
        [HttpGet]
        public string GetGames()
        {
            return gameGrpc.GetGames();
        }

        // POST: api/Game
        [HttpPost]
        public async Task<ActionResult<GameModel>> PostGameAsync([FromBody] GameModel gameModel)
        {
            try
            {
                var result = await gameGrpc.CreateGameAsync(gameModel);
                
                if (result.Equals("Game already exsits"))
                {
                    return  BadRequest("Game already exsits");
                    
                }else if(result.Equals("all atributes cannot be empty"))
                {
                    return  BadRequest("all atributes cannot be empty");
                    
                } else
                {
                    return Created("",gameModel);
                }
                
            }catch(Exception e)
            { 
                return BadRequest();
            }
        }

        // PUT: api/Game/amongUs
        [HttpPut("{name}")]
        public async Task<ActionResult<GameModel>> PutGameAsync(string name, [FromBody] GameModel gameModel)
        {
            try
            {
                var result = await gameGrpc.ModifyGameAsync(name,gameModel);
                
                if (result.Equals("Game already exsits"))
                {
                    return  BadRequest("Game already exsits");
                    
                }else if(result.Equals("all atributes cannot be empty"))
                {
                    return  BadRequest("all atributes cannot be empty");
                    
                } else if (result.Equals("Game not found"))
                {
                    return  BadRequest("Game not found");
                }else
                {
                    return Created("",gameModel);
                }
                
            }catch(Exception e)
            { 
                return BadRequest(e.Message);
            }
        }

        // DELETE: api/Game/pokemon
        [HttpDelete("{game}")]
        public async Task<ActionResult<string>> DeleteGameAsync(string game)
        {
            try
            {
                var result = await gameGrpc.DeleteGameAsync(game);
                    
                if (result.Equals("game cannot be deleted"))
                {
                    return  BadRequest("game cannot be deleted");
                    
                }else if(result.Equals("game title cannot be empty"))
                {
                    return  BadRequest("game title cannot be empty");
                    
                } else if (result.Equals("Game not found"))
                {
                    return  BadRequest("Game not found");
                }else
                {
                    return NoContent();
                }
                
            }catch(Exception e)
            { 
                return BadRequest(e.Message);
            }
        }
    }
}
