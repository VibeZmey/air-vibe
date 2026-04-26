using Flights.Domain.Interfaces;
using MediatR;

namespace Flights.Application.Features.Flights.UpdateFlightStatus;



public class UpdateFlightStatusHandler : IRequestHandler<UpdateFlightStatusCommand, Unit>
{
    private readonly IFlightRepository _flightRepo;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateFlightStatusHandler(
        IFlightRepository flightRepository,
        IUnitOfWork unitOfWork)
    {
        _flightRepo = flightRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Unit> Handle(UpdateFlightStatusCommand request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var flights = await _flightRepo
            .GetFlightsReadyForTimeTransitionsAsync(now, cancellationToken);

        foreach (var flight in flights)
            flight.ApplyScheduledTransitions(now);

        await _unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}