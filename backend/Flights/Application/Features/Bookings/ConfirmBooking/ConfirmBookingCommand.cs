using Flights.Domain.Models;
using MediatR;

namespace Flights.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingCommand : IRequest<Booking>
{
    public Guid BookingId { get; set; }
}