using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;


namespace LogWebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>(); 
                    webBuilder.UseUrls("http://localhost:8000", "https://localhost:8001");
                });
                
        
    }
}