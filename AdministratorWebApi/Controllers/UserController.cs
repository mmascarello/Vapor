using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.GrpcClient;
using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualBasic;

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
        public string GetUsersAsync()
        {
            var response = userGrpc.GetUsers();
            return response;
        }

        [HttpPost]
        public async Task<ActionResult<UserModel>> PostUserAsync([FromBody] UserModel userModel)
        {
            try
            {
                var result = await userGrpc.CreateUserAsync(userModel);


                if (result.Equals("user already exsits"))
                {
                    return BadRequest("user already exsits");
                }
                else if (result.Equals("user and password cannot be empty"))
                {
                    return BadRequest("user and password cannot be empty");
                }
                else
                {
                    return Created("", userModel);
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPut("{name}")]
        public async Task<ActionResult<UserModel>> PutUserAsync(string name, [FromBody] UserModel userModel)
        {
            try
            {
                var result = await userGrpc.ModifyUserAsync(name, userModel);


                if (result.Equals("New user already exsits"))
                {
                    return BadRequest("New user already exsits");
                }
                else if (result.Equals("user and password cannot be empty"))
                {
                    return BadRequest("user and password cannot be empty");
                }
                else
                {
                    return userModel;
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{user}")]
        public async Task<ActionResult<string>> DeleteUserAsync(string user)
        {
            try
            {
                var result = await userGrpc.DeleteUserAsync(user);

                if (result.Equals("New user already exsits"))
                {
                    return BadRequest("New user already exsits");
                }
                else if (result.Equals("user and password cannot be empty"))
                {
                    return BadRequest("user and password cannot be empty");
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpPut("{name} , {game}", Name = "BuyGame")]
        public async Task<ActionResult<string>> BuyGameAsync(string userName, string game)
        {
            try
            {
                var result = await userGrpc.BuyGameAsync(userName, game);

                if (result.Equals("user already has this game!"))
                {
                    return BadRequest("user already has this game!");
                }
                else if (result.Equals("user or game not exists"))
                {
                    return BadRequest("user or game not exists");
                }

                if (result.Equals("game title and username cannot be empty"))
                {
                    return BadRequest("game title and username cannot be empty");
                }
                else
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

        [HttpDelete("{user},{game}", Name = "RefundGame")]
        public async Task<ActionResult<string>> RefundGameAsync(string userName, string gameTitle)
        {
            try
            {
                var result = await userGrpc.RefundGameAsync(userName,gameTitle);
                
                if (result.Equals("user doesn't has this game!)"))
                {
                    return BadRequest("user doesn't has this game!)");
                }
                else if (result.Equals("user or game not exists"))
                {
                    return BadRequest("user or game not exists");
                    
                } else if (result.Equals("game title and username cannot be empty"))
                {
                    return BadRequest("game title and username cannot be empty");
                }
                else
                {
                    return NoContent();
                }
            }
            catch (Exception e)
            {
                return BadRequest();
            }
        }

    }
}