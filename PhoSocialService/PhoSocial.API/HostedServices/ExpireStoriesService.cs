using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Dapper;
using PhoSocial.API.Repositories;

namespace PhoSocial.API.HostedServices
{
    public class ExpireStoriesService : BackgroundService
    {
        private readonly IDbConnectionFactory _db;
        private readonly ILogger<ExpireStoriesService> _logger;
        public ExpireStoriesService(IDbConnectionFactory db, ILogger<ExpireStoriesService> logger) { _db = db; _logger = logger; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ExpireStoriesService started");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var conn = _db.CreateConnection();
                    await conn.ExecuteAsync("EXEC dbo.ExpireStories");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error expiring stories");
                }
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}
