using Flyingdarts.Persistance;

namespace Flyingdarts.Services.X01;

public interface IGameService
{
    public Task<Game> GetGame(long gameId);
    public Task<List<GamePlayer>> GetGamePlayers(long gameId);
    public Task<List<GameDart>> GetGamePlayerGameDarts(long gameId);
    public Task PutGame(Game game);
    public Task PutGamePlayer(GamePlayer gamePlayer);
    public Task PutGameDart(GameDart gameDart);
}