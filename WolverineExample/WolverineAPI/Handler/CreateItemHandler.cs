using Wolverine;
using WolverineAPI.Data;
using WolverineAPI.Messages;

namespace WolverineAPI.Handler;

public class CreateItemHandler
{
    public async Task<ScheduledMessage<ItemCreated>> Handle(CreateItemCommand command, ItemDbContext dbContext)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        dbContext.Items.Add(item);
        //await dbContext.SaveChangesAsync();


        await Console.Out.WriteLineAsync($@"
#######################################################
    ITEM CREATO: {item.Id}
#######################################################
");

        return new ItemCreated((Guid)item.Id).ScheduledAt(command.date);
    }
}
