using GMB.Domain;                                 // Import your domain namespace which contains your DbContext (GMB_DbContext).
using Microsoft.EntityFrameworkCore;             // Import EF Core functionality (e.g. Database.Migrate).
using ILogger = Serilog.ILogger;                  // Create an alias for the Serilog ILogger.

namespace Gaming_multiplayer_backend
{
    /// <summary>
    /// Provides a method for initializing the database by applying migrations.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Initializes the database by applying pending migrations.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to obtain required services.</param>
        /// <exception cref="InvalidOperationException">Thrown when migrations cannot be applied.</exception>
        public static void Initialize(IServiceProvider serviceProvider)
        {
            // Get an instance of the Serilog logger from the service provider.
            var logger = serviceProvider.GetRequiredService<ILogger>();
            logger.Information("Initializing database...");

            // Create a new scope to resolve scoped services (like the DbContext).
            using var scope = serviceProvider.CreateScope();

            // Get an instance of the GMB_DbContext from the service provider.
            var context = scope.ServiceProvider.GetRequiredService<GMB_DbContext>();

            // Apply all pending migrations. If it fails, throw an exception.
            if (!ApplyMigrations(context, logger))
            {
                throw new InvalidOperationException("Database initialization failed. See logs for details.");
            }
        }

        /// <summary>
        /// Applies pending migrations to the database.
        /// </summary>
        /// <param name="context">The database context.</param>
        /// <param name="logger">The logger to record the progress.</param>
        /// <returns>True if migrations were applied successfully; otherwise, false.</returns>
        private static bool ApplyMigrations(GMB_DbContext context, ILogger logger)
        {
            try
            {
                logger.Information("Applying migrations...");
                // Apply all pending migrations to the database.
                context.Database.Migrate();
                logger.Information("Migrations applied successfully.");
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception if migration fails.
                logger.Error(ex, "An error occurred while applying migrations.");
                return false;
            }
        }
    }
}
