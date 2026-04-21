using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using Flights.Infrastructure.Persistence;
using MediatR;

namespace Flights.Application.Features.Bookings.CreateBooking;

public class CreateBookingHandler : IRequestHandler<CreateBookingCommand, Unit>
{
    private readonly IFlightRepository _flightRepo;
    private readonly IPassengerRepository _passengerRepo;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBookingHandler(IFlightRepository flightRepository,
        IPassengerRepository passengerRepository,
        IUnitOfWork unitOfWork,
        IBookingRepository bookingRepository,
        FlightsDbContext context)
    {
        _flightRepo = flightRepository;
        _passengerRepo = passengerRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Unit> Handle(CreateBookingCommand request, CancellationToken cancellationToken)
    {
        var flight = await _flightRepo
            .GetByIdWithDetailsAsync(request.FlightId, cancellationToken);
        if(flight is null)
            throw new ApplicationException("Flight not found");

        var passenger = await _passengerRepo
            .GetByIdAsync(request.PassengerId, cancellationToken);
        if(passenger is null)
            throw new ApplicationException("Passenger not found");

        var booking = Booking.Create(
            request.UserId, 
            passenger, 
            request.SeatNumber, 
            flight);
        
        if(request.IsBusiness) booking
            .UpgradeToBusiness(flight.BusinessPrice, passenger.Type);
        
        if(request.HasFood) 
            booking.AddFood(flight.FoodPrice);
        
        if(request.HasLuggage) 
            booking.AddLuggage(flight.LuggagePrice);
        
        flight.AddBooking(booking);
        await _unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}