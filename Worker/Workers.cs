using System;
using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Worker.Jobs;

namespace Worker
{
    public class Workers : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        //bağımlılığa erişmek için kullandım
        public Workers(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Worker başlatıldı (Job'lar Hangfire tarafından yönetiliyor)");
            return Task.CompletedTask;
        }
    }
}
