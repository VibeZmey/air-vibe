using Flights.Domain.Interfaces;
using Flights.Domain.Models;

namespace Flights.Domain.Events;

public class CheckInOpenedEvent : IDomainEvent
{
    public Guid FLightId { get; set; }
    public FlightStatus NewStatus { get; set; }
    public string FlightNumber { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime CreatedAt { get; set; }
    
}