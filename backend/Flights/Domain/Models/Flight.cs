using System.Text.Json.Serialization;
using Flights.Domain.Dto;

namespace Flights.Domain.Models;

public class Flight
{
    public Guid Id { get; set; }
    public int DurationMins { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    
    public int TotalSeats { get; set; }
    public int BookedSeats { get; set; } = 0;
    public int BusinessSeats { get; set; }
    public int BookedBusinessSeats { get; set; } = 0;
    public decimal FlightPrice { get; set; }
    public decimal LuggagePrice { get; set; }
    public decimal BusinessPrice { get; set; }
    public decimal FoodPrice { get; set; }
    
    public FlightStatus Status { get; set; }
    
    public int FromAirportId { get; set; }
    public Airport FromAirport { get; set; }
    
    public int ToAirportId { get; set; }
    public Airport ToAirport { get; set; }
    
    public int AirplaneId { get; set; }
    public Airplane Airplane { get; set; }
    
    public ICollection<Booking> Bookings { get; set; }

    public void AddBooking(Booking booking)
    {
        if (Status != FlightStatus.Scheduled || Status != FlightStatus.CheckIn)
            throw new ApplicationException("Cannot add booking when Status is not Scheduled");
        
        if(booking.Status != BookingStatus.Confirmed)
            throw new ApplicationException("Cannot add booking when Status is not Confirmed");
        
        if(BookedSeats == TotalSeats)
            throw new ApplicationException("Cannot add booking when the plane is full");

        if (booking.IsBusiness)
        {
            if(BookedBusinessSeats == BusinessSeats)
                throw new ApplicationException("Cannot add booking when the business is full");
            BookedBusinessSeats++;
        }
            
        BookedSeats++;
    }
    
    public static FlightSegment ToFlightSegment(Flight flight)
    {
        return new FlightSegment()
        {
            Id = flight.Id,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            DurationMins = flight.DurationMins,
            FlightPrice = flight.FlightPrice,
            BusinessPrice = flight.BusinessPrice,
            AirportFrom = new AirportDto()
            {
                City = flight.FromAirport.City,
                CountryName = flight.FromAirport.CountryName,
                Code = flight.FromAirport.Code,
            },
            AirportTo = new AirportDto()
            {
                City = flight.ToAirport.City,
                CountryName = flight.ToAirport.CountryName,
                Code = flight.ToAirport.Code,
            },
        };
    }
}

public enum FlightStatus
{
    Scheduled,
    CheckIn,
    Boarding,
    Departed,
    InAir,
    Landed,
    Arrived,
    Cancelled,
    Delayed
}