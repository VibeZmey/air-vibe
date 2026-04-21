using MediatR;

namespace Flights.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommand : IRequest<Unit>
{
    public Guid UserId { get; set; }
    public Guid PassengerId { get; set; }
    public string SeatNumber { get; set; }
    public Guid FlightId { get; set; }
    public bool HasLuggage { get; set; } = false;
    public bool HasFood { get; set; } = false;
    public bool IsBusiness { get; set; } = false; 
}