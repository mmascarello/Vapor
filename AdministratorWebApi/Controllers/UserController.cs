using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AdministrationWebApi.Models;
using AdministratorWebApi.GrpcClient;
using Microsoft.AspNetCore.Mvc;

namespace AdministratorWebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class UserController: ControllerBase
    {
        private UserGrpc userGrpc;

        public UserController(UserGrpc userGrpc)
        {
            this.userGrpc = userGrpc;
        }
        
        [HttpGet]
        public string Get()
        {
           var response = userGrpc.GetUsers();
           return response;
        }    
        
        
    }
}