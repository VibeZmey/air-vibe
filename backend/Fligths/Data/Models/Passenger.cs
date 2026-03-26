using System.Text.Json.Serialization;

namespace Fligths.Data.Models;

public class Passenger
{
    public Guid Id { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid DocumentId { get; set; }
    [JsonIgnore]
    public Document Document { get; set; }
    
    public Guid UserId { get; set; }
}