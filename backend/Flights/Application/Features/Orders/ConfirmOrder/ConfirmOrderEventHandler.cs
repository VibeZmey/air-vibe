using Flights.Domain.Events;
using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using MassTransit;
using MediatR;
using SharedContracts.Messages;

namespace Flights.Application.Features.Orders.ConfirmOrder;

public class ConfirmOrderEventHandler : INotificationHandler<OrderConfirmedEvent>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationRepository _notifyRepo;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly ILogger<ConfirmOrderEventHandler> _logger;
    
    public ConfirmOrderEventHandler(
        IUnitOfWork unitOfWork,
        INotificationRepository notifyRepo,
        IPublishEndpoint publishEndpoint,
        ILogger<ConfirmOrderEventHandler> logger)
    {
        _notifyRepo = notifyRepo;
        _unitOfWork = unitOfWork;
        _publishEndpoint = publishEndpoint;
        _logger = logger;
    }
    
    public async Task Handle(OrderConfirmedEvent notification, CancellationToken cancellationToken)
    {
        //TODO: websocket rabbitmq
        var notify = Notification.CreateOrderConfirmed(notification);
        
        await _publishEndpoint.Publish(new OrderConfirmed
        {
            OrderId = notification.OrderId,
            TotalPrice = notification.TotalPrice,
            Email = notification.Email
        }, cancellationToken);
        
        _logger.LogInformation($"Order {notification.OrderId} has been confirmed");
        await _notifyRepo.AddAsync(notify, cancellationToken);
    }
}