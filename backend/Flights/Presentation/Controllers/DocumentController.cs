using Flights.Application.Features.Documents.CreateDocument;
using Flights.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Authorization;

namespace Flights.Presentation.Controllers;


[ApiController]
[Authorize]
[Route("docs")]
public class DocumentController : ControllerBase
{
    private readonly IMediator _mediator;
    
    public DocumentController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpPost]
    [Authorize(Roles = "User")]
    public async Task<ActionResult<CreateDocumentDto>> CreatePassenger([FromBody] CreateDocumentCommand command)
    {
        var res = await _mediator.Send(command);
        return Ok(res);
    }
}