using JasperFx.Core;
using WolverineAPI.Messages;

namespace Consumer;

public static class ItemCreatedConsumer
{
    public static async Task Consume(ItemCreated itemCreated)
    {
        //await Task.Delay(5000);

        await Console.Out.WriteLineAsync($@"
#############################################
    Faccio qualcosa dopo che:
        {itemCreated.Id}
    è stato creato.
#############################################
");
    }
}
