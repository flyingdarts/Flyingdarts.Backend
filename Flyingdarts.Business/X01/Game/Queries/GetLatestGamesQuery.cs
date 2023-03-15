// ReSharper disable once CheckNamespace
using System.Collections.Generic;

namespace Flyingdarts.Business.X01;
public record GetLatestGamesQuery(long GameId) : IRequest<List<Game>>;
public record GetLatestGamesQueryHandler(X01Service X01Service) : IRequestHandler<GetLatestGamesQuery, List<Game>>
{
    public async Task<List<Game>> Handle(GetLatestGamesQuery request, CancellationToken cancellationToken)
    {
        return await X01Service.GetLatestGames();
    }
}
