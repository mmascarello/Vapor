using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebAPI.Context;
using WebAPI.Services.RabbitMQService;

namespace WebAPI.Services
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> logger;
        private readonly IBus busControl;
        private readonly IServiceProvider serviceProvider;
        
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider){
            this.serviceProvider = serviceProvider;
            this.logger = logger;
            busControl = RabbitHutch.CreateBus("amqps://fhnocqil:3VamHErDywnXy607WYu3QD21i903fFTS@beaver.rmq.cloudamqp.com/fhnocqil");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await busControl.ReceiveAsync<Log>(Queue.ProcessingQueueName, x =>
            {
                Task.Run(() => { ReceiveLog(x); }, stoppingToken);
            });
        }

        private void ReceiveLog(Log log)
        {
            logger.LogInformation("Accion: "+log.Action+", Respuesta: {0}",log.Response);
            
            try
            {
                using (var scope = serviceProvider.CreateScope()) 
                {
                    var db = new LogContext(scope.ServiceProvider.GetRequiredService<DbContextOptions<LogContext>>());
                    db.Logs.Add(log);
                    var addedItems = db.SaveChanges();
                    logger.LogInformation($"Add {addedItems} items");
                }
            }
            catch (Exception e)
            {
                logger.LogInformation($"Exception {e.Message} -> {e.StackTrace}");
            }
        }
    }
}