namespace Flights.Domain.Interfaces;

public interface IDomainEventEmitter
{
    public IReadOnlyCollection<IDomainEvent> Events { get; }
    public void ClearEvents();
}