using MediatR;
using Flights.Domain.Models;

namespace Flights.Application.Features.Passengers.CreatePassenger;

public record CreatePassengerCommand : IRequest<CreatePassengerDto>
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public PassengerType Type { get; set; }
    public bool IsSaved { get; set; }
    public Guid UserId { get; set; }
}