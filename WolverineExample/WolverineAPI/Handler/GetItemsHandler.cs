using Microsoft.EntityFrameworkCore;
using Wolverine;
using WolverineAPI.Data;

namespace WolverineAPI.Handler;

public class GetItemsHandler
{
    public async Task<Item[]> Handle(GetItemsQuery query, ItemDbContext dbContext)
    {
        var items = await dbContext.Items.Take(10).ToArrayAsync();

        return items;
    }
}
