using Flights.Application.Features.Flights;
using Flights.Domain.Dto;
using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using Flights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flights.Infrastructure.Repositories;

public class FlightRepository : IFlightRepository
{
    private readonly FlightsDbContext _context;

    public FlightRepository(FlightsDbContext context)
    {
        _context = context;
    }
    
    public async Task<IReadOnlyCollection<GetFlightsByFilterDto>> GetFlightsByFilter(SearchFlightsQuery query, CancellationToken ct = default)
    {
        var originAirports = await _context.Airports
            .Where(a => 
                a.City == query.OriginCity)
            .Select(a => a.Id)
            .ToListAsync(ct);
        
        var destAirports = await _context.Airports
            .Where(a => 
                a.City == query.DestinationCity)
            .Select(a => a.Id)
            .ToListAsync(ct);
        
        if (!originAirports.Any() || !destAirports.Any()) return Array.Empty<GetFlightsByFilterDto>();
        
        var baseQuery = _context.Flights.AsNoTracking()
            .Where(f => 
                originAirports.Contains(f.FromAirportId) && 
                destAirports.Contains(f.ToAirportId))
            .Where(f => f.Status == FlightStatus.Scheduled)
            .Include(f => f.ToAirport)
            .Include(f => f.FromAirport);

        int totalPassengers = query.Adults + query.Kids;
        
        var outboundQuery = baseQuery
            .Where(f =>
                f.DepartureTime.Date == query.DepartureDate.Date)
            .Where(f =>
                (f.TotalSeats - f.BookedSeats) >= totalPassengers)
            .Where(f =>
                !query.IsBusinessOnly ||
                (f.BusinessSeats - f.BookedBusinessSeats) >= totalPassengers);
        
        if (query.DepartureHourFrom.HasValue)
            outboundQuery = outboundQuery
                .Where(f => 
                    f.DepartureTime.Hour >= query.DepartureHourFrom.Value);
        if (query.DepartureHourTo.HasValue)
            outboundQuery = outboundQuery
                .Where(f => 
                    f.DepartureTime.Hour <= query.DepartureHourTo.Value);

        if (query.MaxTotalPrice.HasValue)
        {
            outboundQuery = outboundQuery.Where(f => 
                (query.IsBusinessOnly ? 
                    f.FlightPrice+f.BusinessPrice <= query.MaxTotalPrice.Value : 
                    f.FlightPrice <= query.MaxTotalPrice.Value)); 
        }
        
        
        var outboundFlights = await outboundQuery
            .OrderBy(f => f.FlightPrice)
            .Take(query.PageSize * 3)
            .ToListAsync(ct);

        List<Flight> returnFlights = null;
        if (query.ReturnDate.HasValue)
        {
            var returnBaseQuery = _context.Flights.AsNoTracking()
                .Where(f => 
                    destAirports.Contains(f.FromAirportId) && 
                    originAirports.Contains(f.ToAirportId))
                .Where(f => f.Status == FlightStatus.Scheduled)
                .Include(f => f.ToAirport)
                .Include(f => f.FromAirport);

            returnFlights = await returnBaseQuery
                .Where(f => f.DepartureTime.Date == query.ReturnDate.Value.Date)
                .Take(query.PageSize * 3)
                .ToListAsync(ct);
        }
        
        var result = new List<GetFlightsByFilterDto>();

        foreach (var outFlight in outboundFlights)
        {
            if (query.ReturnDate.HasValue && returnFlights.Any())
            {
                foreach (var retFlight in returnFlights)
                {
                    var outboundFlight = Flight.ToFlightSegment(outFlight);
                    var returnFlight = Flight.ToFlightSegment(retFlight);
                    result.Add(new GetFlightsByFilterDto
                    {
                        Outbound = outboundFlight,
                        Return = returnFlight,
                        TotalPrice = (query.IsBusinessOnly ? 
                            (outboundFlight.FlightPrice + outboundFlight.BusinessPrice + 
                             returnFlight.FlightPrice + returnFlight.BusinessPrice)*totalPassengers :
                            outboundFlight.FlightPrice*totalPassengers)
                    });
                }
            }
            else
            {
                var outboundFlight = Flight.ToFlightSegment(outFlight);
                result.Add(new GetFlightsByFilterDto
                {
                    Outbound = Flight.ToFlightSegment(outFlight),
                    Return = null,
                    TotalPrice = (query.IsBusinessOnly ? 
                        (outboundFlight.FlightPrice + outboundFlight.BusinessPrice)*totalPassengers :
                        outboundFlight.FlightPrice*totalPassengers)
                });
            }
        }

        return result
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();
    }

    public async Task<Flight?> GetByIdAsync(Guid flightId, CancellationToken ct = default)
    {
        return await _context.Flights.Include(f=>f.Airplane).FirstOrDefaultAsync(f => f.Id == flightId, ct);
    }
}