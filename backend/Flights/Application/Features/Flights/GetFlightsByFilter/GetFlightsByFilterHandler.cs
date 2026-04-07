using Flights.Domain.Dto;
using Flights.Domain.Interfaces;
using MediatR;

namespace Flights.Application.Features.Flights;

public class GetFlightsByFilterHandler : 
    IRequestHandler<GetFlightsByFilterQuery, IReadOnlyCollection<GetFlightsByFilterDto>>
{
    private readonly IFlightRepository _flightRepo;
    
    public GetFlightsByFilterHandler(IFlightRepository flightRepository)
    {
        _flightRepo = flightRepository;    
    }
    
    public async Task<IReadOnlyCollection<GetFlightsByFilterDto>> Handle(GetFlightsByFilterQuery request, CancellationToken cancellationToken)
    {
        return await _flightRepo
            .GetFlightsByFilter(GetFlightsByFilterQuery
                .ToSearchFlightsQuery(request), 
                cancellationToken);
    }
}