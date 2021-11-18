using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        public void Post([FromBody] string value)
        {
        }

        // PUT: api/Game/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE: api/Game/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
