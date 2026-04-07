using Flights.Domain.Models;

namespace Flights.Domain.Dto;

public class PassengerWithDocumentsDto
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    
    public PassengerType Type { get; set; }
    public ICollection<DocumentDto> Documents { get; set; }
}