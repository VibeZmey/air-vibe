using MediatR;

namespace Flights.Domain.Interfaces;

public interface IDomainEvent : INotification
{
    public DateTime CreatedAt { get; }
}

public interface IDomainEventHandler<in T> : INotificationHandler<T>
    where T : IDomainEvent;
