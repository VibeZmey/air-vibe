using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using MediatR;

namespace Flights.Application.Features.Bookings.CreateBooking;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Unit>
{
    private readonly IFlightRepository _flightRepo;
    private readonly IPassengerRepository _passengerRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IBookingRepository _bookingRepo;

    public CreateBookingHandler(IFlightRepository flightRepository,
        IPassengerRepository passengerRepository,
        IUnitOfWork unitOfWork,
        IBookingRepository bookingRepository)
    {
        _flightRepo = flightRepository;
        _passengerRepo = passengerRepository;
        _unitOfWork = unitOfWork;
        _bookingRepo = bookingRepository;
    }
    
    public async Task<Unit> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var flight = await _flightRepo
            .GetByIdAsync(request.FlightId, cancellationToken);
        
        var passenger = await _passengerRepo
            .GetByIdAsync(request.PassengerId, cancellationToken);
        
        var booking = Booking.Create(
            request.UserId, 
            passenger, 
            request.SeatNumber, 
            flight);
        
        if(request.IsBusiness) booking
            .UpgradeToBusiness(flight.BusinessPrice, passenger.Type);
        
        if(request.HasFood) booking.AddFood(flight.FoodPrice);
        
        if(request.HasLuggage) booking.AddLuggage(flight.LuggagePrice);
        //TODO: сделать оплату тут где то хз
        await _bookingRepo.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}