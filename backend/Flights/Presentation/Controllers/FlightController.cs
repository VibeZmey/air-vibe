using Flights.Application.Features.Flights;
using Flights.Domain.Dto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
namespace Flights.Presentation.Controllers;

[ApiController]
[Authorize]
[Route("flights")]
public class FlightController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public FlightController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IReadOnlyCollection<GetFlightsByFilterDto>>> GetFlightsByFilter([FromQuery] GetFlightsByFilterQuery request, CancellationToken ct = default)
    {
        return Ok(await _mediator.Send(request, ct));
    } 
}