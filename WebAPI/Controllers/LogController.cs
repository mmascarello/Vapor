using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly LogContext context;

        public LogController(LogContext context)
        {
            this.context = context;
        }
        
        // GET: api/Log
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetAll()
        {
            return await context.Logs.ToListAsync();
        }

        // GET: api/Log/5
        [HttpGet("{id}", Name = "Get")]
        public async Task<ActionResult<Log>> GetTodoItem(int id)
        {
            var log = await context.Logs.FindAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return log;
        }

        // POST: api/Log
        [HttpPost]
        public async Task<ActionResult<Log>> PostLog([FromBody] Log log)
        {
            context.Logs.Add(log);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTodoItem), new { id = log.Id }, log);
        }
        
        /*// PUT: api/Log/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }*/

        // PUT: api/TodoItem/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTodoItem(int id,[FromBody] Log log)
        {
            if (id != log.Id)
            {
                return BadRequest();
            }

            context.Entry(log).State = EntityState.Modified;

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        
        
        // DELETE: api/Log/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Log>> DeleteLog(int id)
        {
            var log = await context.Logs.FindAsync(id);
            if (log == null)
            {
                return NotFound();
            }

            context.Logs.Remove(log);
            await context.SaveChangesAsync();

            return log;
        }
        private bool LogExists(int id)
        {
            return context.Logs.Any(l => l.Id == id);
        }
    }
}
