using Flights.Domain.Models;

namespace Flights.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IReadOnlyCollection<Notification>> GetByUserId(Guid userId);
    
    Task AddRangeAsync(IReadOnlyCollection<Notification> notifications, 
        CancellationToken ct = default);

    Task AddAsync(Notification notification, 
        CancellationToken ct = default);
}