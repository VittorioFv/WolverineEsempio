using EFinfrastructure;
using MediatR;

namespace MediatRAPI.Handler;

public record CreateItemCommand(Item Item) : IRequest;

