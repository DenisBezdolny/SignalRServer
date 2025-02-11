using GMB.Domain.Configurations;      // Import custom configuration classes for entities (Room and Client).
using GMB.Domain.Entities;           // Import the entity classes.
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core functionalities.

namespace GMB.Domain
{
    /// <summary>
    /// Represents the database context for the Gaming Multiplayer Backend.
    /// This class is responsible for interacting with the database using Entity Framework Core.
    /// </summary>
    public class GMB_DbContext : DbContext
    {
        /// <summary>
        /// Gets or sets the DbSet of Room entities.
        /// This property allows CRUD operations on the Rooms table.
        /// </summary>
        public DbSet<Room> Rooms { get; set; }

        /// <summary>
        /// Gets or sets the DbSet of Client entities.
        /// This property allows CRUD operations on the Clients table.
        /// </summary>
        public DbSet<Client> Clients { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GMB_DbContext"/> class.
        /// </summary>
        /// <param name="options">The options to be used by a <see cref="DbContext"/>.</param>
        public GMB_DbContext(DbContextOptions<GMB_DbContext> options)
            : base(options){}

        /// <summary>
        /// Configures the entity mappings and relationships using the ModelBuilder.
        /// This method applies custom configurations for entities.
        /// </summary>
        /// <param name="modelBuilder">The builder being used to construct the model for the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Call the base implementation to ensure any base configurations are applied.
            base.OnModelCreating(modelBuilder);

            // Apply the custom configuration for the Room entity.
            modelBuilder.ApplyConfiguration(new RoomConfiguration());

            // Apply the custom configuration for the Client entity.
            modelBuilder.ApplyConfiguration(new ClientConfiguration());
        }
    }
}
