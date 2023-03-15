// ReSharper disable once CheckNamespace
namespace Flyingdarts.Business.X01;

public record GetGameQuery(long GameId) : IRequest<Game>;
public record GetGameQueryHandler(X01Service X01Service) : IRequestHandler<GetGameQuery, Game>
{
    public async Task<Game> Handle(GetGameQuery request, CancellationToken cancellationToken)
    {
        return await X01Service.GetGame(request.GameId);
    }
}
