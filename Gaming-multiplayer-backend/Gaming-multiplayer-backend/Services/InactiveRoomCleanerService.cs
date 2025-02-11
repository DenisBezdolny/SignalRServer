using GMB.Domain;                                     // Import the domain namespace (contains your DbContext and entity definitions).
using Microsoft.EntityFrameworkCore;                  // Import EF Core functionality for database operations.
using Microsoft.Extensions.Hosting;                   // For BackgroundService.
using Microsoft.Extensions.Logging;                   // For logging.

namespace Gaming_multiplayer_backend.Services
{
    /// <summary>
    /// A background service that periodically cleans up inactive rooms from the database.
    /// Inactive rooms are defined as those with no clients.
    /// </summary>
    public class InactiveRoomCleanerService : BackgroundService
    {
        // Service provider to resolve scoped dependencies (such as the DbContext).
        private readonly IServiceProvider _serviceProvider;

        // Logger for logging information and errors.
        private readonly ILogger<InactiveRoomCleanerService> _logger;

        // Time interval between clean-up executions (10 minutes in this case).
        private readonly TimeSpan _cleanInterval = TimeSpan.FromMinutes(10);

        /// <summary>
        /// Constructor for InactiveRoomCleanerService.
        /// </summary>
        /// <param name="serviceProvider">The service provider to create scopes for services.</param>
        /// <param name="logger">The logger for logging messages.</param>
        public InactiveRoomCleanerService(IServiceProvider serviceProvider, ILogger<InactiveRoomCleanerService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// This method is called when the service starts. It runs a loop until cancellation is requested.
        /// Within the loop, it calls the method to clean inactive rooms and then waits for the specified interval.
        /// </summary>
        /// <param name="stoppingToken">A token that is signaled when the service should stop.</param>
        /// <returns>A Task that represents the long-running operation.</returns>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Continue running until a cancellation is requested.
            while (!stoppingToken.IsCancellationRequested)
            {
                // Clean up inactive rooms from the database.
                await CleanInactiveRooms();

                // Wait for the defined interval before the next cleanup.
                await Task.Delay(_cleanInterval, stoppingToken);
            }
        }

        /// <summary>
        /// Cleans inactive rooms from the database.
        /// Inactive rooms are those that do not contain any clients.
        /// </summary>
        /// <returns>A Task representing the asynchronous operation.</returns>
        private async Task CleanInactiveRooms()
        {
            // Create a new scope to get a new instance of the DbContext.
            using var scope = _serviceProvider.CreateScope();

            // Resolve the DbContext from the scope.
            var dbContext = scope.ServiceProvider.GetRequiredService<GMB_DbContext>();

            // Retrieve all rooms that have no associated clients.
            var inactiveRooms = await dbContext.Rooms
                .Where(r => !r.Clients.Any())
                .ToListAsync();

            // If there are any inactive rooms, remove them from the database.
            if (inactiveRooms.Any())
            {
                // Remove the inactive rooms from the context.
                dbContext.Rooms.RemoveRange(inactiveRooms);

                // Save the changes to the database.
                await dbContext.SaveChangesAsync();

                // Log the number of rooms removed.
                _logger.LogInformation($"Deleted {inactiveRooms.Count} inactive rooms.");
            }
        }
    }
}
