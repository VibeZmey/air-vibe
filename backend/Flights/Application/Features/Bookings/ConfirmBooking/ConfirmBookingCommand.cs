using Flights.Domain.Models;
using MediatR;

namespace Flights.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingCommand : IRequest<Unit>
{
    public Guid BookingId { get; set; }
}