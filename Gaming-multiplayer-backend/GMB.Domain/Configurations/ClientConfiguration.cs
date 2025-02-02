using GMB.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GMB.Domain.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(c => c.ConnectionId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.PublicIp)
                   .HasMaxLength(45);

            builder.Property(c => c.PublicPort);

            builder.HasIndex(c => c.ConnectionId)
                   .HasDatabaseName("IX_Client_ConnectionId")
                   .IsUnique();

            builder.HasOne(c => c.Room) 
                   .WithMany(r => r.Clients)
                   .HasForeignKey(c => c.RoomId)
                   .OnDelete(DeleteBehavior.Cascade); 
        }
    }
}
