using GMB.Domain;
using Microsoft.EntityFrameworkCore;

namespace Gaming_multiplayer_backend.Services
{
    public class InactiveClientCleanerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InactiveClientCleanerService> _logger;
        private readonly TimeSpan _cleanInterval = TimeSpan.FromMinutes(10);

        public InactiveClientCleanerService(IServiceProvider serviceProvider, ILogger<InactiveClientCleanerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanInactiveClients();
                await Task.Delay(_cleanInterval, stoppingToken);
            }
        }

        private async Task CleanInactiveClients()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GMB_DbContext>();

            var inactiveClients = await dbContext.Clients
                .Where(c => string.IsNullOrEmpty(c.ConnectionId)) // Проверка на пустое или null ConnectionId
                .ToListAsync();

            if (inactiveClients.Any())
            {
                dbContext.Clients.RemoveRange(inactiveClients);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Удалено {inactiveClients.Count} неактивных клиентов.");
            }
        }
    }
}
