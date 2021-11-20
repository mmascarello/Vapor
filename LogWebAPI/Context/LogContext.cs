using Domain;
using Microsoft.EntityFrameworkCore;

namespace LogWebAPI.Context
{
    public class LogContext:DbContext
    {
        public LogContext(DbContextOptions<LogContext> options)
            : base(options)
        {
        }

        public DbSet<Log> Logs { get; set; } = null!;
    }
}