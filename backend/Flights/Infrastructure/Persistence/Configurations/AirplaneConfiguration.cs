using Flights.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Flights.Infrastructure.Persistence.Configurations;

public class AirplaneConfiguration : IEntityTypeConfiguration<Airplane>
{
    public void Configure(EntityTypeBuilder<Airplane> builder)
    {
        builder.HasKey(a => a.Id);
        
        builder
            .HasOne(a => a.Airline)
            .WithMany(a => a.Airplanes)
            .HasForeignKey(a => a.AirlineId);
    }
}