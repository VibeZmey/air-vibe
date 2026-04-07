using Flights.Application.Features.Bookings.CreateBooking;
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
    
    public  BookingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task CreateBooking([FromBody] CreateBookingCommand request, CancellationToken cancellationToken)
    {
        await _mediator.Send(request, cancellationToken);
    }
}