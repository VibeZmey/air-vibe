using Flights.Domain.Interfaces;
using Flights.Domain.Models;
using Flights.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Flights.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly FlightsDbContext _context;

    public OrderRepository(FlightsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        await _context.Orders.AddAsync(order, ct);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Bookings)
            .Include(o => o.Flight)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        _context.Orders.Update(order);
        await Task.CompletedTask;
    }
}