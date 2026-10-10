using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using WorldPinball.Logistics.Domain.Entities;
namespace WorldPinball.Logistics.Infrastructure.Persistence.Configurations;

public sealed class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.ToTable("shipments");
        //shipment_id
        builder.HasKey(x => x.ShipmentId);
        
        //order_id
        builder.Property(x => x.OrderId)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(x => x.OrderId)
            .IsUnique();
        //destination_address
        builder.Property(x => x.DestinationAddress)
            .HasMaxLength(500)
            .IsRequired();
        
        //status
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        
        // tracking_number
        builder.Property(x => x.TrackingNumber)
            .HasMaxLength(100);
        
        //cancellation_reason
        builder.Property(x => x.CancellationReason)
            .HasMaxLength(500);
        builder.Property(x => x.MachineId);
        
        // Optimistic concurrency token (PostgreSQL system column xmin)
        builder.Property<uint>("xmin")
            .IsRowVersion();
    }

    
    
}