using System.Text.Json.Serialization;

namespace Fligths.Data.Models;

public class Document
{
    public Guid Id { get; set; }
    public string DocumentType { get; set; }
    
    public string FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string LastName { get; set; }
    
    public string Number { get; set; }
    public string? Series { get; set; }
    
    public string Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime? ValidityPeriod { get; set; }
    
    public Guid PassengerId { get; set; }
    [JsonIgnore]
    public Passenger Passanger { get; set; }
    
    public Guid UserId { get; set; }
}