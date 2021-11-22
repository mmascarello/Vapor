using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc;
using LogWebAPI.BusinessLogic;
using LogWebAPI.Models;

namespace LogWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly LogLogic logLogic;
        public LogController(LogLogic logLogic)
        {
            this.logLogic = logLogic;
        }

        // GET: api/Log/
        [HttpGet]
        public async Task<ActionResult<List<Log>>> GetFilteredLogs([FromQuery]LogModel logModel)
        {
                var log= logModel.ToEntity();
                var logs = await logLogic.GetFiltered(log);
                return logs; 
        }
    }
}
