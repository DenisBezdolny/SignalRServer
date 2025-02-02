using GMB.Domain;
using Microsoft.EntityFrameworkCore;
using ILogger = Serilog.ILogger;

namespace Gaming_multiplayer_backend
{
    public static class DbInitializer
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger>();
            logger.Information("Initializing database...");

            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<GMB_DbContext>();

            if (!ApplyMigrations(context, logger))
            {
                throw new InvalidOperationException("Database initialization failed. See logs for details.");
            }
        }

        private static bool ApplyMigrations(GMB_DbContext context, ILogger logger)
        {
            try
            {
                logger.Information("Applying migrations...");
                context.Database.Migrate();
                logger.Information("Migrations applied successfully.");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "An error occurred while applying migrations.");
                return false;
            }
        }
    }
}
