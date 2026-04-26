using Flights.Domain.Interfaces;

namespace Flights.Domain.Events;

public record BookingConfirmedEvent : IDomainEvent
{
    public Guid BookingId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; } 
    public string Currency { get; set; }   
    public DateTime CreatedAt { get; }
}
