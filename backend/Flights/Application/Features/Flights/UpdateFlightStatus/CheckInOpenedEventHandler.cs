using Flights.Domain.Events;
using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using MassTransit;
using SharedContracts.Messages;

namespace Flights.Application.Features.Flights.UpdateFlightStatus;

public class CheckInOpenedEventHandler : IDomainEventHandler<CheckInOpenedEvent>
{
    private readonly IFlightRepository _flightRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notifyRepo;
    private readonly IPublishEndpoint _publishEndpoint;

    public CheckInOpenedEventHandler(
        IFlightRepository flightRepository, 
        IUnitOfWork unitOfWork,
        INotificationRepository notificationRepository,
        IPublishEndpoint publishEndpoint)
    {
        _flightRepo = flightRepository;
        _unitOfWork = unitOfWork;
        _notifyRepo = notificationRepository;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task Handle(CheckInOpenedEvent notification, CancellationToken cancellationToken)
    {
        if(notification.NewStatus != FlightStatus.CheckIn)
            throw new ApplicationException("CheckInOpenedEvent is not valid");
        
        var userIds = await _flightRepo.GetUserIdsByFlightIdAsync(notification.FLightId, cancellationToken);
        
        if (!userIds.Any())
            return;

        var notifications = userIds
            .Select(userId =>
                Notification.CreateCheckInOpened(notification, userId))
            .ToList();
        
        await _publishEndpoint.Publish(new CheckInOpened
        {
            StartTime =  notification.StartTime,
            FlightNumber = notification.FlightNumber,
            EndTime = notification.EndTime,
            DepartureTime = notification.DepartureTime
        }, cancellationToken);
        
        await _notifyRepo.AddRangeAsync(notifications, cancellationToken);
        await _unitOfWork.SaveAsync(cancellationToken);
    }
}