using GMB.Domain.Configurations;
using GMB.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace GMB.Domain
{
    public class GMB_DbContext : DbContext
    {
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Client> Clients { get; set; }

        public GMB_DbContext(DbContextOptions<GMB_DbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new RoomConfiguration());
            modelBuilder.ApplyConfiguration(new ClientConfiguration());

        }


    }
}
