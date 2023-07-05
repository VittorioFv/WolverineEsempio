using EFinfrastructure;
using MediatR;

namespace MediatRAPI.Handler;
public record class GetItemsQuery() : IRequest<Item[]>;
