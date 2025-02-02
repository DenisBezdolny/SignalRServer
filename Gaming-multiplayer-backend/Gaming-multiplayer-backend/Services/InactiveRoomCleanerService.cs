using GMB.Domain;
using Microsoft.EntityFrameworkCore;

namespace Gaming_multiplayer_backend.Services
{
    public class InactiveRoomCleanerService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<InactiveRoomCleanerService> _logger;
        private readonly TimeSpan _cleanInterval = TimeSpan.FromMinutes(10); 

        public InactiveRoomCleanerService(IServiceProvider serviceProvider, ILogger<InactiveRoomCleanerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await CleanInactiveRooms();
                await Task.Delay(_cleanInterval, stoppingToken);
            }
        }

        private async Task CleanInactiveRooms()
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<GMB_DbContext>();

            var inactiveRooms = await dbContext.Rooms
                .Where(r => !r.Clients.Any()) 
                .ToListAsync();

            if (inactiveRooms.Any())
            {
                dbContext.Rooms.RemoveRange(inactiveRooms);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Удалено {inactiveRooms.Count} неактивных комнат.");
            }
        }
    }
}
