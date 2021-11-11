using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Mvc;
using WebAPI.BusinessLogic;
using WebAPI.Models;

namespace WebAPI.Controllers
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
            try
            {
                var log= logModel.ToEntity();
                var logs = await logLogic.GetFiltered(log);
                if (logs.Count == 0) return NotFound();
                return logs;
            }
            catch (Exception e)
            {
                return NotFound();
            }
        }
    }
}
