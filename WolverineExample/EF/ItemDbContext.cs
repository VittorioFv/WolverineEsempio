using Microsoft.EntityFrameworkCore;

namespace EFinfrastructure;

public class ItemDbContext : DbContext
{
    public ItemDbContext(DbContextOptions<ItemDbContext> options) : base(options)
    {
    }

    public ItemDbContext() { }

    public virtual DbSet<Item> Items { get; set; }
}
