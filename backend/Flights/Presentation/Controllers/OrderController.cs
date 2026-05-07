using System.Security.Claims;
using Flights.Application.Features.Orders.ConfirmOrder;
using Flights.Application.Features.Orders.CreateOrder;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Flights.Presentation.Controllers;


[ApiController]
[Route("orders")]
public class OrderController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrderController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderCommand request,
        CancellationToken ct)
    {
        await _mediator.Send(request, ct);
        return Ok();
    }
    
    [HttpPost("confirm/{id:guid}")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<IActionResult> ConfirmOrder([FromRoute] Guid id,
        CancellationToken ct)
    {
        var command = new ConfirmOrderCommand()
        {
            OrderId = id,
            Email = User.FindFirst(ClaimTypes.Email).Value
        };
        await _mediator.Send(command, ct);
        return Ok();
    }
}