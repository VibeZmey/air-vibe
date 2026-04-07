using Flights.Application.Features.Passengers.CreatePassenger;
using Flights.Application.Features.Passengers.GetPassengersWithDocumentsByUserId;
using Flights.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flights.Presentation.Controllers;

[ApiController]
[Route("passengers")]
public class PassengerController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public PassengerController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<ActionResult<IReadOnlyCollection<PassengerWithDocumentsDto>>> GetPassengersWithDocuments(CancellationToken cancellationToken)
    {
        var userId = User.FindFirst("userId").Value;
        var res = await _mediator.Send(
            new GetPassengersWithDocumentsByUserIdQuery
            {
                UserId = Guid.Parse(userId)
            }, 
            cancellationToken);
        return Ok(res);
    }
    
    
    [HttpPost]
    public async Task<ActionResult<CreatePassengerDto>> CreatePassenger([FromBody] CreatePassengerCommand passenger)
    {
        
        var res = await _mediator.Send(passenger);
        return Ok(res);
    }
}