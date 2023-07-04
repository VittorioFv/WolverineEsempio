using WolverineAPI.Data;

namespace WolverineAPI.Handler;

public record CreateItemCommand(Item Item, DateTimeOffset date);
