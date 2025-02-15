using Gaming_multiplayer_backend.Services;             // Application-specific services
using GMB.BLL.Contracts;                                 // Business logic layer contracts (interfaces)
using GMB.BLL.Services;                                  // Business logic layer service implementations
using GMB.Domain.Repositories.Interfaces;                // Domain repository interfaces
using GMB.Domain.Repositories.Repositories;              // Domain repository concrete implementations
            
namespace Gaming_multiplayer_backend.Extensions
{
    // This static class provides an extension method to register application services
    // into the dependency injection container.
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all application services and repositories with the dependency injection container.
        /// </summary>
        /// <param name="services">The IServiceCollection to add services to.</param>
        /// <returns>The updated IServiceCollection with all registered services.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            // Register a generic repository so that any request for IRepository<T> is served by Repository<T>.
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Register specific repository implementations for clients and rooms.
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();

            // Register a hosted service that cleans inactive rooms in the background.
            services.AddHostedService<InactiveRoomCleanerService>();

            // Register business logic services that provide higher-level operations for rooms and clients.
            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IClientService, ClientService>();

            // Register and configure SignalR to enable real-time communication,
            // and enable detailed errors for troubleshooting.
            services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
            });

            // Add support for Swagger/OpenAPI to automatically generate API documentation.
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            // Return the service collection so that further chaining is possible.
            return services;
        }
    }
}
