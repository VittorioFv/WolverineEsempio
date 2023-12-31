﻿using EFinfrastructure;
using Wolverine;
using Wolverine.Attributes;
using WolverineAPI.Messages;

namespace WolverineAPI.Handler;

[Transactional]
public static class CreateItemHandler
{
    public static async Task<ItemCreated> Handle(
        CreateItemCommand command, 
        ItemDbContext dbContext)
    {
        Item item = command.Item;
        item.Id = Guid.NewGuid();

        dbContext.Items.Add(item);
        
        await Console.Out.WriteLineAsync($@"

    ITEM CREATO: {item.Id}

");

        return new ItemCreated((Guid)item.Id);
            //.ScheduledAt(DateTimeOffset.Now.AddSeconds(10));
    }
}
