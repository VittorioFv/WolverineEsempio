using EFinfrastructure;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
using WolverineAPI.Handler;

namespace WolverineAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private IMessageBus _bus;

    public ItemController(IMessageBus bus)
    {
        _bus = bus;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        return Ok(await _bus.InvokeAsync<Item[]>(new GetItemsQuery()));
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem(Item item)
    {
        if (item.Name == "")
        {
            return BadRequest();
        }

        var command = new CreateItemCommand(item);

        await _bus.InvokeAsync(command);

        return Ok();
    }
}