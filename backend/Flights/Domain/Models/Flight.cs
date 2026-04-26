using System.Text.Json.Serialization;
using Flights.Domain.Dto;
using Flights.Domain.Events;
using Flights.Domain.Interfaces;

namespace Flights.Domain.Models;

public class Flight : IDomainEventEmitter
{
    public Guid Id { get; set; }
    public string FlightNumber { get; set; } = string.Empty;
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
    public ICollection<Booking> Bookings { get; set; } = [];
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyCollection<IDomainEvent> Events => _events.AsReadOnly();
    public void ClearEvents() => _events.Clear();
    
    public void ApplyScheduledTransitions(DateTime now)
    {
        if (Status == FlightStatus.Scheduled && 
            now.AddHours(24) >= DepartureTime && now < DepartureTime)
        {
            Status = FlightStatus.CheckIn;
            //TODO: протестить всё, на сегодня хватит
            //TODO: добавить ивент на отправку уведомления на сайте и на почту, так же и для всех остальных
            _events.Add(new CheckInOpenedEvent()
            {
                FLightId = Id,
                DepartureTime = DepartureTime,
                StartTime = DepartureTime.AddHours(-24),
                EndTime = DepartureTime.AddMinutes(-30),
                FlightNumber = FlightNumber,
                NewStatus = Status,
                CreatedAt = DateTime.UtcNow
            });
            Console.WriteLine(nameof(FlightStatus.CheckIn));
            return;
        }

        if (Status == FlightStatus.CheckIn &&
            now.AddMinutes(30) >= DepartureTime && now < DepartureTime)
        {
            Status = FlightStatus.Boarding;
            Console.WriteLine(nameof(FlightStatus.Boarding));
            return;
        }

        if (Status == FlightStatus.Boarding &&
            now >= DepartureTime && now < ArrivalTime)
        {
            Status = FlightStatus.Departed;
            Console.WriteLine(nameof(FlightStatus.Departed));
            return;
        }

        if (Status == FlightStatus.Departed &&
            now >= ArrivalTime)
        {
            Status = FlightStatus.Arrived;
            Console.WriteLine(nameof(FlightStatus.Arrived));
            return;
        }
    }
    
    public static FlightDto ToDto(Flight flight)
    {
        return new FlightDto()
        {
            Id = flight.Id,
            DurationMins = flight.DurationMins,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            TotalSeats = flight.TotalSeats,
            BookedSeats = flight.BookedSeats,
            BusinessSeats = flight.BusinessSeats,
            BookedBusinessSeats = flight.BookedBusinessSeats,
            FlightPrice = flight.FlightPrice,
            LuggagePrice = flight.LuggagePrice,
            BusinessPrice = flight.BusinessPrice,
            FoodPrice = flight.FoodPrice,
            Status = flight.Status,
            FromAirport = new AirportDto()
            {
                City = flight.FromAirport.City,
                CountryName = flight.FromAirport.CountryName,
                Code = flight.FromAirport.Code,
            },
            ToAirport = new AirportDto()
            {
                City = flight.ToAirport.City,
                CountryName = flight.ToAirport.CountryName,
                Code = flight.ToAirport.Code,
            },
            Airplane = new AirplaneDto()
            {
                BuisnessColumns = flight.Airplane.BuisnessColumns,
                BuisnessRows = flight.Airplane.BuisnessRows,
                Columns = flight.Airplane.Columns,
                Rows = flight.Airplane.Rows,
                Name = flight.Airplane.Name,
                SpacePlusRow = flight.Airplane.SpacePlusRow,
                AirlineId = flight.Airplane.AirlineId
            },
            Bookings = flight.Bookings.Select(b => b.SeatNumber).ToList()
        };
    }

    public void AddBooking(Booking booking)
    {
        if(Bookings.Any(b => b.SeatNumber == booking.SeatNumber && 
                             (b.Status == BookingStatus.Confirmed || b.Status == BookingStatus.Pending)))
            throw new ApplicationException("This seat is already in bookings");
        
        if (Status != FlightStatus.Scheduled && Status != FlightStatus.CheckIn)
            throw new ApplicationException("Cannot add booking when Status is not Scheduled");
        Console.WriteLine("BEFORE ADD BOOKING");
        Bookings.Add(booking);
    }
    public static FlightForBookingDto ToFlightForBooking(Flight flight)
    {
        return new FlightForBookingDto()
        {
            Id = flight.Id,
            DepartureTime = flight.DepartureTime,
            ArrivalTime = flight.ArrivalTime,
            DurationMins = flight.DurationMins,
            AirlineName = flight.Airplane.Airline.Name,
            FromAirport = new AirportDto()
            {
                City = flight.FromAirport.City,
                CountryName = flight.FromAirport.CountryName,
                Code = flight.FromAirport.Code,
            },
            ToAirport = new AirportDto()
            {
                City = flight.ToAirport.City,
                CountryName = flight.ToAirport.CountryName,
                Code = flight.ToAirport.Code,
            },
        };
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
            AirlineName = flight.Airplane.Airline.Name,
            FromAirport = new AirportDto()
            {
                City = flight.FromAirport.City,
                CountryName = flight.FromAirport.CountryName,
                Code = flight.FromAirport.Code,
            },
            ToAirport = new AirportDto()
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
    Arrived,
    Cancelled,
    Delayed
}