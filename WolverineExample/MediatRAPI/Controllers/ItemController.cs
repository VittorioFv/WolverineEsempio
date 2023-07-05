using EFinfrastructure;
using MediatR;
using MediatRAPI.Handler;
using Microsoft.AspNetCore.Mvc;

namespace WolverineAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ItemController : ControllerBase
{
    private IMediator _mediator;

    public ItemController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetItems()
    {
        return Ok(await _mediator.Send(new GetItemsQuery()));
    }

    [HttpPost]
    public async Task<IActionResult> CreateItem(Item item)
    {
        if (item.Name == "")
        {
            return BadRequest();
        }

        var command = new CreateItemCommand(item);

        await _mediator.Send(command);

        return Ok();
    }
}