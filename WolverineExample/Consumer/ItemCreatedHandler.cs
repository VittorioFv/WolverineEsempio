using WolverineAPI.Messages;

namespace Consumer;

public class ItemCreatedHandler
{
    public async Task Handle(ItemCreated itemCreated)
    {
        await Console.Out.WriteLineAsync($@"
#############################################
    Faccio qualcosa dopo che:
        {itemCreated.Id}
    è stato creato.
#############################################
");
    }
}
