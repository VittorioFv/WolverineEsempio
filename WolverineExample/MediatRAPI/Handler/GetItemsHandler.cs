using EFinfrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace MediatRAPI.Handler;

public class GetItemsHandler : IRequestHandler<GetItemsQuery, Item[]>
{
    ItemDbContext _dbContext;

    public GetItemsHandler(ItemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Item[]> Handle(GetItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await _dbContext.Items.OrderBy(x => x.Name).Take(10).ToArrayAsync();

        return items;
    }
}
