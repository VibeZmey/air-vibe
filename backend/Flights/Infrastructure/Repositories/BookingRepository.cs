using Flights.Domain.Models;
using Flights.Infrastructure.Persistence;

namespace Flights.Infrastructure.Repositories;

public class BookingRepository
{
    private readonly FlightsDbContext _context;

    public BookingRepository(FlightsDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        await _context.Bookings.AddAsync(booking, ct);
    }

    public async Task<Booking?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Bookings.FindAsync(id, ct);
    }
}