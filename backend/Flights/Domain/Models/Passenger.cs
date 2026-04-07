using System.Text.Json.Serialization;

namespace Flights.Domain.Models;

public class Passenger
{
    public Guid Id { get; private set; }
    public string? Email { get; private set; }
    public string? PhoneNumber { get; private set; }
    
    public PassengerType Type { get; private set; }
    public ICollection<Document> Documents { get; private set; }
    public ICollection<Booking> Bookings { get; private set; }
    
    public Guid UserId { get; private set; }
    

    public static Passenger Create(
        Guid userId,
        PassengerType type,
        string? email = null,
        string? phoneNumber = null)
    {
        //TODO: Валидация почты и телефона
        return new Passenger
        {
            Id = Guid.NewGuid(),
            Type = type,
            UserId = userId,
            PhoneNumber = phoneNumber,
            Email = email,
        };
    }
}

public enum PassengerType
{
    Adult,
    Kid,
    Baby
}