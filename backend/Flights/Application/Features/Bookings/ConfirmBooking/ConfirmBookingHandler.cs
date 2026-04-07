using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using MediatR;

namespace Flights.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, Booking>
{
    private readonly IBookingRepository _bookingRepo;
    private readonly IFlightRepository _flightRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ConfirmBookingHandler(IBookingRepository bookingRepository,
        IFlightRepository flightRepository,
        IUnitOfWork unitOfWork)
    {
        _bookingRepo = bookingRepository;
        _flightRepo = flightRepository;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<Booking> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
            throw new ApplicationException("Booking not found");
        
        var flight = await _flightRepo.GetByIdAsync(booking.FlightId, cancellationToken);
        if (flight is null)
            throw new ApplicationException("Flight not found");
        
        booking.ConfirmPayment();
        await _bookingRepo.AddAsync(booking, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
        return booking;
    }
}