using EFinfrastructure;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;

namespace WolverineAPI.Handler;

public static class GetItemsHandler
{
    public static async Task<Item[]> Handle(GetItemsQuery query, ItemDbContext dbContext)
    {
        Item[] items = await dbContext.Items.OrderBy(x => x.Name).Take(10).ToArrayAsync();

        // This code is only an example
        // Return items trigger the cascading message
        return items;
    }
}
