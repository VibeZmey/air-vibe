using MediatR;

namespace Flights.Application.Features.Bookings.CreateBooking;

public class CreateBookingCommand : IRequest<Unit>
{
    public Guid UserId { get; private set; }
    public Guid PassengerId { get; private set; }
    public string SeatNumber { get; private set; }
    public Guid FlightId { get; private set; }
    public bool HasLuggage { get; private set; } = false;
    public bool HasFood { get; private set; } = false;
    public bool IsBusiness { get; private set; } = false; 
}