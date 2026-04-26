using Flights.Application.Common.Interfaces;
using Flights.Domain.Interfaces;
using Flights.Infrastructure.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;

namespace Flights.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly FlightsDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(
        FlightsDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveAsync(CancellationToken ct = default)
    {
        var entries = _context.ChangeTracker
            .Entries<IDomainEventEmitter>().ToList();
        
        foreach (var entry in entries)
        {
            foreach (var evt in entry.Entity.Events)
            {
                var outboxMsg = EventSerializer.Serialize(evt);
                _context.OutboxMessages.Add(outboxMsg);
            }
            entry.Entity.ClearEvents();
        }
        
        return await _context.SaveChangesAsync(ct);
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}