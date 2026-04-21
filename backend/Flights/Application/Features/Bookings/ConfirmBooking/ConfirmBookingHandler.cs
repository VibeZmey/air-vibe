using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using MediatR;

namespace Flights.Application.Features.Bookings.ConfirmBooking;

public class ConfirmBookingHandler : IRequestHandler<ConfirmBookingCommand, Unit>
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
    
    public async Task<Unit> Handle(ConfirmBookingCommand request, CancellationToken cancellationToken)
    {
        var booking = await _bookingRepo.GetByIdAsync(request.BookingId, cancellationToken);
        if (booking is null)
            throw new ApplicationException("Booking not found");
        
        booking.Confirm();
        await _unitOfWork.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}