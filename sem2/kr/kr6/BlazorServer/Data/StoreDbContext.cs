using Microsoft.EntityFrameworkCore;
using Shared;

namespace BlazorServer.Data;

public class StoreDbContext(DbContextOptions<StoreDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
