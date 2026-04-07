using System.Text.Json.Serialization;
using Flights.Domain.Exceptions;

namespace Flights.Domain.Models;

public class Booking
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid PassengerId { get; private set; }
    public Passenger Passenger { get; private set; }
    public string SeatNumber { get; private set; }
    public Guid FlightId { get; private set; }
    public Flight Flight { get; private set; }
    
    public bool HasLuggage { get; private set; } = false;
    public bool HasFood { get; private set; } = false;
    public bool IsBusiness { get; private set; } = false;
    public decimal TotalPrice { get; private set; }
    public BookingStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Booking Create(
        Guid userId,
        Passenger passenger,
        string seatNumber,
        Flight flight)
    {
        if(flight is null)
            throw new DomainException("Flight not found");
        if(passenger is null)
            throw new DomainException("Passenger not found");

        decimal price = flight.FlightPrice;
        switch (passenger.Type)
        {
            case PassengerType.Kid:
                price /= 2;
                break;
            case PassengerType.Baby:
                price = 0;
                break;
        }
        
        return new Booking
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            PassengerId = passenger.Id,
            FlightId = flight.Id,
            SeatNumber = seatNumber,
            Status = BookingStatus.Pending,
            TotalPrice = price,
            CreatedAt = DateTime.UtcNow,
        };
    }
    
    public void AddLuggage(decimal luggagePrice)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Confirmed bookings cannot be changed");
            
        if (HasLuggage) return;
        
        HasLuggage = true;
        TotalPrice += luggagePrice;
    }

    public void AddFood(decimal foodPrice)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Confirmed bookings cannot be changed");

        if (HasFood) return;

        HasFood = true;
        TotalPrice += foodPrice;
    }
    
    public void UpgradeToBusiness(decimal upgradePrice, PassengerType passengerType)
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Confirmed bookings cannot be changed");

        if (IsBusiness) return;

        IsBusiness = true;
        
        switch (passengerType)
        {
            case PassengerType.Adult:
                TotalPrice += upgradePrice;
                break;
            case PassengerType.Kid:
                TotalPrice += upgradePrice/2;
                break;
        }
    }

    public void ConfirmPayment()
    {
        if (Status != BookingStatus.Pending)
            throw new DomainException("Reservation has already been processed");

        Status = BookingStatus.Confirmed;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
            throw new DomainException("Already canceled");
            
        if (Status == BookingStatus.FlightDeparted)
            throw new DomainException("The flight has already departed");

        Status = BookingStatus.Cancelled;
    }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Cancelled,
    FlightDeparted
}