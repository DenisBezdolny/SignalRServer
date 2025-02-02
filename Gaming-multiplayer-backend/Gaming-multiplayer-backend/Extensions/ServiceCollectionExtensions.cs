using Gaming_multiplayer_backend.Services;
using GMB.BLL.Contracts;
using GMB.BLL.Services;
using GMB.Domain.Repositories.Interfaces;
using GMB.Domain.Repositories.Repositories;
using GMB.Domain.Repositories.Reposytories;

namespace Gaming_multiplayer_backend.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers all application services and repositories.
        /// </summary>
        /// <param name="services">The service collection to configure.</param>
        /// <returns>The configured service collection.</returns>
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IRoomRepository, RoomRepository>();
            services.AddHostedService<InactiveRoomCleanerService>();
            services.AddHostedService<InactiveClientCleanerService>();


            services.AddScoped<IRoomService, RoomService>();
            services.AddScoped<IClientService, ClientService>();

            services.AddSignalR(options => {
                options.EnableDetailedErrors = true;
            });
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }
    }
}
