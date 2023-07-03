using Microsoft.EntityFrameworkCore;
using Wolverine.EntityFrameworkCore;

namespace WolverineAPI.Data;

public class ItemDbContext : DbContext
{
    public ItemDbContext(DbContextOptions<ItemDbContext> options) : base(options)
    {
    }

    public DbSet<Item> Items { get; set; }
}
