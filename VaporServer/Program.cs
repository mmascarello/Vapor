using System;
using System.Threading.Tasks;
using VaporServer.Factory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;


namespace VaporServer
{
    class Program
    {
        private static Startup _startup = new Startup();

        static async Task Main(string[] args)
        {   
            Console.Write("Iniciando Server con Sockets...");
            await Task.Run(async () =>  await _startup.Start().ConfigureAwait(false));
            
            Console.Write("Iniciando Server con GRPC...");
            await Task.Run(async () => await CreateHostBuilder(args).Build().RunAsync().ConfigureAwait(false));
            
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureKestrel(options =>
                    {
                        options.ListenLocalhost(6001, o => o.Protocols = 
                            HttpProtocols.Http2);
                    });
                    webBuilder.UseStartup<GrpcStartup>();
                });
    }
}