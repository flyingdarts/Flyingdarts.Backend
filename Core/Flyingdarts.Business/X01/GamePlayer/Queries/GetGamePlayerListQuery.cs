// ReSharper disable once CheckNamespace
using System.Collections.Generic;

namespace Flyingdarts.Business.X01;
public record GetGamePlayerListQuery(long GameId) : IRequest<List<Game>>;
public record GetGamePlayerListQueryHandler(X01Service X01Service) : IRequestHandler<GetGamePlayerListQuery, List<Game>>
{
    public async Task<List<Game>> Handle(GetGamePlayerListQuery request, CancellationToken cancellationToken)
    {
        return await X01Service.GetLatestGames();
    }
}
