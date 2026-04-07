using Flights.Domain.Models;

namespace Flights.Application.Features.Passengers.CreatePassenger;

public class CreatePassengerDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public PassengerType Type { get; set; }
    public Guid UserId { get; set; }

    public static CreatePassengerDto Map(Passenger passenger)
    {
        return new CreatePassengerDto
        {
            Id = passenger.Id,
            Email = passenger.Email,
            PhoneNumber = passenger.PhoneNumber,
            Type = passenger.Type,
            UserId = passenger.UserId
        };
    }
}