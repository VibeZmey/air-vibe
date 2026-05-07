using Flights.Application.Features.Bookings.CancelBooking;
using Flights.Application.Features.Bookings.ConfirmBooking;
using Flights.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flights.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("books")]
public class BookingController : ControllerBase
{
    private readonly IMediator _mediator;

    public BookingController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost("confirm-batch")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<IActionResult> ConfirmBookingsBatch(
        [FromBody] ConfirmBookingsCommand command, 
        CancellationToken ct)
    {
        await _mediator.Send(command, ct);
        return Ok();
    }
    
    [HttpPatch("{id:guid}/cancel")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<IActionResult> CancelBooking([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var command = new CancelBookingCommand()
        {
            BookingId = id
        };
        await _mediator.Send(command, cancellationToken);
        return Ok();
    }
}
    