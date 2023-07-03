using WolverineAPI.Messages;

namespace WolverineAPI.Handler;

public class CreateItemHandler
{
    public async Task<ItemCreated> Handle(CreateItemCommand command)
    {
        Guid IdItem = Guid.NewGuid();

        await Console.Out.WriteLineAsync($@"
#############################################
    ITEM CREATO: {IdItem}
#############################################
");

        return new ItemCreated(IdItem);
    }
}
