using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;
using WebAPI.Context;

namespace WebAPI.BusinessLogic
{
    public class LogLogic
    {
        private readonly LogContext context;

        public LogLogic(LogContext context)
        {
            this.context = context;
        }

        public async Task<List<Log>> GetFiltered(Log log)
        {
            var logs = await context.Logs.ToListAsync();
            
            if (log.User != null)
            {
                logs = logs.Where(l => l.User == log.User).ToList();
            }

            if (log.Game != null)
            {
                logs = logs.Where(l => l.Game == log.Game).ToList();
            }

            if (log.Date.ToShortDateString() != Convert.ToDateTime("01/01/0001 00:00:00").ToShortDateString())
            {
                logs = logs.Where(l => l.Date.ToShortDateString() == log.Date.ToShortDateString()).ToList();
            }
          
            return logs;
        }
    }
}