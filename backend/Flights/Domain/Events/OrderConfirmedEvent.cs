using Flights.Domain.Interfaces;

namespace Flights.Domain.Events;

public record OrderConfirmedEvent : IDomainEvent
{
    public Guid OrderId { get; set; }
    public string FlightNumber { get; set; }
    public Guid UserId { get; set; }
    public string Email { get; set; }
    public decimal TotalPrice { get; set; } 
    public DateTime CreatedAt { get; set; }
}