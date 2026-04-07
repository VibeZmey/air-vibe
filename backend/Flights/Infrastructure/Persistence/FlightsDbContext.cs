using Flights.Domain.Models;
using Flights.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Flights.Infrastructure.Persistence;

public class FlightsDbContext(DbContextOptions<FlightsDbContext> options) 
    : DbContext(options)
{
    public DbSet<Passenger> Passengers { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Flight> Flights { get; set; }
    public DbSet<Airport> Airports { get; set; }
    public DbSet<Airline> Airlines { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Airplane> Airplanes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AirlineConfiguration());
        modelBuilder.ApplyConfiguration(new AirplaneConfiguration());
        modelBuilder.ApplyConfiguration(new AirportConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new DocumentConfiguration());
        modelBuilder.ApplyConfiguration(new FlightConfiguration());
        modelBuilder.ApplyConfiguration(new PassengerConfiguration());
        
        base.OnModelCreating(modelBuilder);
    }
}
