using Microsoft.EntityFrameworkCore;

namespace Fligths.Data;

public class FligthsDbContext(DbContextOptions<FligthsDbContext> options) 
    : DbContext(options)
{
    
}
