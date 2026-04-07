using Flights.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flights.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder
            .HasIndex(b => 
                new { b.FlightId, b.SeatNumber }) 
            .IsUnique()
            .HasDatabaseName("IX_Flight_Seat");  
        
        builder.HasKey(b => b.Id);
    }
}