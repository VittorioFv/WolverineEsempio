using EFinfrastructure;

namespace WolverineAPI.Handler;

public record CreateItemCommand(Item Item, DateTimeOffset date);
