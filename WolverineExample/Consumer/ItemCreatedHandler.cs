using WolverineAPI.Messages;

namespace Consumer;

public class ItemCreatedConsumer
{
    public async Task Consumer(ItemCreated itemCreated)
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
