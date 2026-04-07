using Flights.Domain.Dto;
using Flights.Domain.Models;

namespace Flights.Domain.Interfaces;

public interface IFlightRepository
{
    Task<IReadOnlyCollection<GetFlightsByFilterDto>> GetFlightsByFilter(SearchFlightsQuery query,
        CancellationToken ct = default);

    Task<Flight?> GetByIdAsync(Guid flightId, CancellationToken ct = default);
}