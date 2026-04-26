using Flights.Domain.Events;
using Flights.Domain.Interfaces;
using Flights.Domain.Models;

namespace Flights.Application.Features.Flights.UpdateFlightStatus;

public class CheckInOpenedEventHandler : IDomainEventHandler<CheckInOpenedEvent>
{
    private readonly IFlightRepository _flightRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notifyRepo;

    public CheckInOpenedEventHandler(
        IFlightRepository flightRepository, 
        IUnitOfWork unitOfWork,
        INotificationRepository notificationRepository)
    {
        _flightRepo = flightRepository;
        _unitOfWork = unitOfWork;
        _notifyRepo = notificationRepository;
    }
    
    public async Task Handle(CheckInOpenedEvent notification, CancellationToken cancellationToken)
    {
        if(notification.NewStatus != FlightStatus.CheckIn)
            throw new ApplicationException("CheckInOpenedEvent is not valid");
        
        var flight = await _flightRepo
            .GetByIdWithDetailsAsync(notification.FLightId, cancellationToken);
        
        if(flight == null)
            throw new ApplicationException($"Flight with id {notification.FLightId} not found");

        var userIds = flight.Bookings.Select(b => b.UserId);
        
        var notifications = new List<Notification>();
        
        foreach (var userId in userIds)
            notifications.Add(Notification
                .CreateCheckInOpened(notification, userId));
        
        await _notifyRepo.AddRangeAsync(notifications, cancellationToken);
        //TODO: тут будет отправка на фронт через WebSocket, после добавления в бд
        await _unitOfWork.SaveAsync(cancellationToken);
    }
}