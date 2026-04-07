namespace Flights.Domain.Dto;

public record GetFlightsByFilterDto
{
    public FlightSegment Outbound { get; set; }
    public FlightSegment? Return { get; set; }
    public decimal TotalPrice { get; set; }
}

public record FlightSegment
{
    public Guid Id { get; init; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int DurationMins { get; set; }
    public decimal FlightPrice { get; set; }
    public decimal BusinessPrice { get; set; }
    public AirportDto AirportFrom { get; set; }
    public AirportDto AirportTo { get; set; }
}

public record SearchFlightsQuery
{
    public DateTime DepartureDate { get; init; }
    public DateTime? ReturnDate { get; init; }   
    
    public string OriginCity { get; init; } 
    public string DestinationCity { get; init; } 
    
    public int Adults { get; init; } = 1;
    public int Kids { get; init; } 
    public int Babies { get; init; }  
    
    public decimal? MaxTotalPrice { get; init; }
    
    public int? DepartureHourFrom { get; init; } 
    public int? DepartureHourTo { get; init; } 
    
    public Guid? AirlineId { get; init; }
    
    public bool IsBusinessOnly { get; init; }
    public int PageSize { get; init; }
    public int PageNumber { get; init; }
}