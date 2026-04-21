using Flights.Application.Features.Documents.CreateDocument;
using Flights.Application.Features.Documents.DeleteDocumet;
using Flights.Application.Features.Documents.UpdateDocument;
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
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<ActionResult<CreateDocumentDto>> CreatePassenger(
        [FromBody] CreateDocumentCommand command, 
        CancellationToken ct)
    {
        var res = await _mediator.Send(command, ct);
        return Ok(res);
    }

    [HttpPatch("{id:guid}")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<ActionResult<UpdateDocumentDto>> UpdateDocument([FromRoute] Guid id, [FromBody] UpdateDocumentCommand command, CancellationToken ct)
    {
        command.Id = id;
        return await _mediator.Send(command, ct);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin, User, Supporter")]
    public async Task<IActionResult> DeleteDocument([FromRoute] Guid id, CancellationToken ct)
    {
        var command = new DeleteDocumentCommand()
        {
            DocumentId = id
        };
        await _mediator.Send(command, ct);
        return Ok();
    }
    
}