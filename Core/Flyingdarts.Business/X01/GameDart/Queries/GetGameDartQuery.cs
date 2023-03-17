// ReSharper disable once CheckNamespace
namespace Flyingdarts.Business.X01;

public record GetGameDartQuery(long GameId) : IRequest<List<GameDart>>;
public record GetGameDartQueryHandler(X01Service X01Service) : IRequestHandler<GetGameDartQuery, List<GameDart>>
{
    public async Task<List<GameDart>> Handle(GetGameDartQuery request, CancellationToken cancellationToken)
    {
        return await X01Service.GetGamePlayerGameDarts(request.GameId);
    }
}
