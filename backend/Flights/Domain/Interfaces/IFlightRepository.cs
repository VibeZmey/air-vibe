using Flights.Domain.Dto;
using Flights.Domain.Models;

namespace Flights.Domain.Interfaces;

public interface IFlightRepository
{
    Task<IReadOnlyCollection<GetFlightsByFilterDto>> GetFlightsByFilter(SearchFlightsQuery query,
        CancellationToken ct = default);

    Task<Flight?> GetByIdWithDetailsAsync(Guid flightId, 
        CancellationToken ct = default);
    
    Task<HashSet<Guid>> GetUserIdsByFlightIdAsync(Guid flightId,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<Flight>> GetFlightsReadyForTimeTransitionsAsync(DateTime now,
        CancellationToken ct = default);

    Task<IReadOnlyCollection<Guid>?> GetUsersIdsByFlightId(Guid id, 
        CancellationToken ct = default);
}