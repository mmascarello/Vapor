using System.Threading.Tasks;
using AdministratorWebApi.Models;
using AdministratorWebApi.GrpcClient;
using Microsoft.AspNetCore.Mvc;


namespace AdministratorWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private UserGrpc userGrpc;

        public UserController(UserGrpc userGrpc)
        {
            this.userGrpc = userGrpc;
        }

        [HttpGet]
        public async Task<ActionResult<string>> GetUsersAsync([FromQuery(Name = "title")] string title)
        {
            var response = "";
            if (string.IsNullOrEmpty(title))
            {
                response = await userGrpc.GetUsers();
            }
            else
            {
                response = await userGrpc.GetUsersForAGameAsync(title);
            }
            return response;
        }
        

        [HttpPost]
        public async Task<ActionResult<string>> PostUserAsync([FromBody] UserModel userModel)
        {
            var result = await userGrpc.CreateUserAsync(userModel);
            return Created("", result);
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<string>> PutUserAsync(string name, [FromBody] UserModel userModel)
        {
            var result = await userGrpc.ModifyUserAsync(name, userModel);
            return result;
        }

        [HttpDelete("{user}")]
        public async Task<ActionResult<string>> DeleteUserAsync(string user)
        {
            var result = await userGrpc.DeleteUserAsync(user);
            return result;
        }

        [HttpPut]
        public async Task<ActionResult<string>> BuyGameAsync([FromQuery]string userName, [FromQuery] string gameTitle)
        {
            var result = await userGrpc.BuyGameAsync(userName, gameTitle);
            return result;
       }

        [HttpDelete]
        public async Task<ActionResult<string>> RefundGameAsync([FromQuery]string userName,[FromQuery] string gameTitle)
        {
            var result = await userGrpc.RefundGameAsync(userName,gameTitle);
            return result;
           
        }

    }
}