using Flights.Application.Features.Bookings.GetBookingsByUserId;
using Flights.Domain.Dto;
using Flights.Domain.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flights.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public UserController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}/bookings")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<ActionResult<IReadOnlyCollection<BookingDto>>> GetBookings([FromRoute] Guid id,
        CancellationToken ct)
    {
        var query = new GetBookingsByUserIdQuery()
        {
            UserId = id
        };
        return Ok(await _mediator.Send(query, ct));
    }
}