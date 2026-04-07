using Flights.Domain.Interfaces;
using Flights.Infrastructure.Persistence;
using Flights.Infrastructure.Repositories;
using Flights.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Flights.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<FlightsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IPassengerRepository, PassengerRepository>();
        services.AddScoped<IEncryptionService, EncryptionService>();
        services.AddScoped<IFlightRepository, FlightRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}