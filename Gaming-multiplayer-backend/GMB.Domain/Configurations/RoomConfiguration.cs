using GMB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMB.Domain.Configurations
{
    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasIndex(r => r.Code)
                   .HasDatabaseName("IX_Room_Code")
                   .IsUnique(); 

            builder.Property(r => r.MaxPlayers)
                   .IsRequired();

            builder.Property(r => r.IsPrivate) 
                   .IsRequired();

            builder.Property(r => r.IsActive)
                   .IsRequired();

            builder.HasMany(r => r.Clients)
                   .WithOne(c => c.Room) 
                   .HasForeignKey(c => c.RoomId)  
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.RowVersion)
                   .IsRowVersion();
        }
    }
}
