using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using Flights.Infrastructure.Persistence;

namespace Flights.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly FlightsDbContext _context;

    public NotificationRepository(FlightsDbContext context)
    {
        _context = context;
    }
    
    public Task<IReadOnlyCollection<Notification>> GetByUserId(Guid userId)
    {
        throw new NotImplementedException();
    }

    public async Task AddRangeAsync(IReadOnlyCollection<Notification> notifications, CancellationToken ct = default)
    {
        await _context.Notifications
            .AddRangeAsync(notifications, ct);
    }
}