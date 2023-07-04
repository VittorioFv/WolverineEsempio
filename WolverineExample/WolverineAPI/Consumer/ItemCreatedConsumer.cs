using JasperFx.Core;
using WolverineAPI.Messages;

namespace Consumer;

public class ItemCreatedConsumer
{
    public async Task Consume(ItemCreated itemCreated)
    {
        await Console.Out.WriteLineAsync($@"
#############################################
    Inizio lavoro su {itemCreated.Id}
#############################################
");
        await Task.Delay(1000);

        await Console.Out.WriteLineAsync($@"
#############################################
    Faccio qualcosa dopo che:
        {itemCreated.Id}
    è stato creato.
#############################################
");
    }
}
