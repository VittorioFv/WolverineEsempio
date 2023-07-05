using EFinfrastructure;
using Microsoft.EntityFrameworkCore;

namespace WolverineAPI.Handler;

public static class GetItemsHandler
{
    public static async Task<Item[]> Handle(GetItemsQuery query, ItemDbContext dbContext)
    {
        var items = await dbContext.Items.OrderBy(x => x.Name).Take(10).ToArrayAsync();

        return items;
    }
}
