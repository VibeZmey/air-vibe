using Flights.Domain.Models;
using Flights.Infrastructure.Persistence;
using GeoTimeZone;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace Flights.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("test")]
public class TestController
{
    public FlightsDbContext _context;
    
    public TestController(FlightsDbContext dbContext)
    {
        _context = dbContext;
    }
    
    [HttpPost("settimezoneoffset")]
    public async Task SetTimezoneOffsetSettings()
    {
        var res = await _context.Airports.ToListAsync();
        foreach(var i in res)
        {
            string timeZoneId = TimeZoneLookup.GetTimeZone(i.Latitude, i.Longitude).Result;
            var timeZone = DateTimeZoneProviders.Tzdb[timeZoneId];
            var now = SystemClock.Instance.GetCurrentInstant();
            var offset = timeZone.GetUtcOffset(now);
            i.TimezoneOffset = (int)offset.ToTimeSpan().TotalHours;
        }
        await _context.SaveChangesAsync();
    }

    [HttpPost("airline")]
    public async Task CreateAirline(int id, string name, double coef)
    {
        await _context.Airlines.AddAsync(new Airline(){Id = id, Name = name, Coefficient = coef});
        await _context.SaveChangesAsync();
    }
    
    [HttpPost("airplane")]
    public async Task CreateAirplane(int Id, string Name, int Rows, int Columns, int BuisnessRows, int BuisnessColumns,int SpacePlusRow,int AirlineId)
    {
        await _context.Airplanes.AddAsync(new Airplane()
        {
            Id = Id,
            Name = Name,
            Rows = Rows,
            Columns = Columns,
            BuisnessRows = BuisnessRows,
            BuisnessColumns = BuisnessColumns,
            SpacePlusRow = SpacePlusRow,
            AirlineId = AirlineId
        });
        await _context.SaveChangesAsync();
    }
    

[HttpPost("flights")]
public async Task GenerateAirline(int count, string country, string? fromCity, string? toCity, DateTime from, DateTime to, int minuteStep = 60)
{
    var airplanes = _context.Airplanes.Include(a => a.Airline).ToList();
    
    for (int i = 0; i < count; i++)
    {
        var c = await _context.Airports.CountAsync(p => p.CountryName == country);
        if (c == 0) continue;

        var ap1 = fromCity is null
            ? await _context.Airports.Where(p => p.CountryName == country)
                .Skip(Random.Shared.Next(0, c))
                .FirstOrDefaultAsync()
            : await _context.Airports.FirstAsync(p => p.CountryName == country && p.City == fromCity);

        var ap2 = toCity is null
            ? await _context.Airports.Where(p => p.CountryName == country)
                .Skip(Random.Shared.Next(0, c))
                .FirstOrDefaultAsync()
            : await _context.Airports.FirstAsync(p => p.CountryName == country && p.City == toCity);

        if (ap1 is null || ap2 is null) continue;

        var dist = Test.CalculateDistance(ap1.Latitude, ap1.Longitude, ap2.Latitude, ap2.Longitude);
        var dur = (int)((dist / 800) * 60);
        if (dur < 60) continue;

        var depUtc = Test.GenerateRoundedDateTimeUtc(from, to, minuteStep);
        var arrUtc = depUtc.AddMinutes(dur);


        decimal fp = dist switch
        {
            < 1500 => ((decimal)dist) / 15,
            < 3000 => ((decimal)dist) / 12,
            < 5000 => ((decimal)dist) / 7,
            _      => ((decimal)dist) / 5
        };

        var airplane = airplanes[Random.Shared.Next(0, airplanes.Count)];
        var flight = new Flight
        {
            Id = Guid.NewGuid(),
            Number = Guid.NewGuid().ToString().Substring(0, 4).ToUpper(),
            FromAirportId = ap1.Id,
            ToAirportId = ap2.Id,
            DurationMins = dur,
            DepartureTime = depUtc,
            ArrivalTime = arrUtc,
            FlightPrice = Math.Round(fp, 2) * (decimal)airplane.Airline.Coefficient,
            LuggagePrice = Random.Shared.Next(25, 100),
            BusinessPrice = Math.Round(fp * 2, 2) * (decimal)airplane.Airline.Coefficient,
            FoodPrice = Random.Shared.Next(10, 30),
            Status = FlightStatus.Scheduled,
            AirplaneId = airplane.Id,
            TotalSeats = airplane.Rows * airplane.Columns,
            BusinessSeats = airplane.BuisnessRows * airplane.BuisnessColumns,
            BookedBusinessSeats = 0,
            BookedSeats = 0,
        };

        await _context.Flights.AddAsync(flight);
    }

    await _context.SaveChangesAsync();
}
    [HttpDelete("flights")]
    public async Task DeleteFlights()
    {
        _context.Flights.RemoveRange(_context.Flights);
        await _context.SaveChangesAsync();
    }

    [HttpDelete("outbox")]
    public async Task DeleteOutbox()
    {
        _context.OutboxMessages.RemoveRange(_context.OutboxMessages);
        await _context.SaveChangesAsync();
    }
}

public class Test
{
    private static readonly Random random = new Random();
    
    public static DateTime GenerateRoundedDateTimeUtc(
        DateTime minDateUtc, 
        DateTime maxDateUtc, 
        int minuteStep = 60)
    {
        if (minDateUtc.Kind != DateTimeKind.Utc) minDateUtc = minDateUtc.ToUniversalTime();
        if (maxDateUtc.Kind != DateTimeKind.Utc) maxDateUtc = maxDateUtc.ToUniversalTime();
    
        var date = minDateUtc.AddDays(Random.Shared.Next(0, (maxDateUtc - minDateUtc).Days + 1));
        var hour = Random.Shared.Next(0, 24);
        var minute = Random.Shared.Next(0, 60 / minuteStep) * minuteStep;
    
        return new DateTime(date.Year, date.Month, date.Day, hour, minute, 0, DateTimeKind.Utc);
    }
    
    public static async Task<Airport> GenerateAirportId(FlightsDbContext _context, string? country)
    {
        var count = await _context.Airports
            .CountAsync(p => p.CountryName == country);
    
        var skip = Random.Shared.Next(0, count);
    
        return await _context.Airports
            .Where(p => p.CountryName == country)
            .Skip(skip)
            .FirstOrDefaultAsync();
    }
    
    private const double EarthRadiusKm = 6371.0;
    
    public static double CalculateDistance(
        double lat1, double lon1, 
        double lat2, double lon2)
    {
        var lat1Rad = ToRadians(lat1);
        var lat2Rad = ToRadians(lat2);
        var deltaLat = ToRadians(lat2 - lat1);
        var deltaLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return EarthRadiusKm * c;
    }
    private static double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180.0;
    }

}