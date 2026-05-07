using Flights.Domain.Events;
using Flights.Domain.Interfaces;

namespace Flights.Domain.Models;

public class Order : IDomainEventEmitter
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid FlightId { get; set; }
    public Flight Flight { get; set; }
    public ICollection<Booking> Bookings { get; set; } = [];
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalPrice { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    private readonly List<IDomainEvent> _events = [];
    public IReadOnlyCollection<IDomainEvent> Events => _events.AsReadOnly();
    public void ClearEvents() => _events.Clear();

    public static Order Create(Guid flightId, Guid userId)
    {
        return new Order()
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FlightId = flightId,
        };
    }
    
    public void AddBooking(Booking booking)
    { 
        Bookings.Add(booking);
        TotalPrice += booking.TotalPrice;
    }
    
    public OrderStatus Confirm(string email)
    {
        if (DateTime.UtcNow >= CreatedAt.AddMinutes(15) 
            && Status == OrderStatus.Pending)
        {
            Status = OrderStatus.Expired;
            return Status;
        }
        
        // if(Status == OrderStatus.Confirmed)
        //     return OrderStatus.Confirmed;
        //
        // if(Status == OrderStatus.Cancelled)
        //     return OrderStatus.Cancelled;
        
        
        
        foreach (var booking in Bookings)
            booking.Confirm();
        
        _events.Add(new OrderConfirmedEvent()
        {
            OrderId = Id,
            UserId = this.UserId,
            Email = email,
            TotalPrice = this.TotalPrice,
            FlightNumber = Flight.Number,
            CreatedAt = DateTime.UtcNow
        });
        return Status;
    }
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Cancelled,
    Expired
}