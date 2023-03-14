using Flyingdarts.Persistance;

namespace Flyingdarts.Services.X01;

public interface IGameService
{
    public Game GetGame(long gameId);
    public void PutGame(Game game);
    public void PutGamePlayer(GamePlayer gamePlayer);
    public void PutGameDart(GameDart gameDart);
}