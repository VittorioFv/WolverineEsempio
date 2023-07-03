using JasperFx.Core;
using Wolverine;
using WolverineAPI.Data;
using WolverineAPI.Messages;

namespace WolverineAPI.Handler;

public class CreateItemHandler
{
    public async Task<OutgoingMessages> Handle(CreateItemCommand command, ItemDbContext dbContext)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        dbContext.Items.Add(item);
        await dbContext.SaveChangesAsync();


        await Console.Out.WriteLineAsync($@"
#############################################
    ITEM CREATO: {item.Id}
#############################################
");
        OutgoingMessages messages = new OutgoingMessages();

        messages.Schedule(new ItemCreated((Guid)item.Id), DateTimeOffset.Now.AddSeconds(10));

        return messages;
    }
}
