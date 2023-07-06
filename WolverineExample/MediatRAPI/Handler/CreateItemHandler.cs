using EFinfrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace MediatRAPI.Handler;

public class CreateItemHandler : IRequestHandler<CreateItemCommand>
{
    ItemDbContext _dbContext;

    public CreateItemHandler(ItemDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(CreateItemCommand command, CancellationToken cancellationToken)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        _dbContext.Items.Add(item);
        await _dbContext.SaveChangesAsync();

        await Console.Out.WriteLineAsync($@"

    ITEM CREATO: {item.Id}

");

        // await Task.Delay(5000);

        await Console.Out.WriteLineAsync($@"

    Faccio qualcosa dopo che:
        {item.Id}
    è stato creato.

");
    }
}
