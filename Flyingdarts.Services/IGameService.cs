using Flyingdarts.Persistance;

namespace Flyingdarts.Services.X01;

public interface IGameService
{
    public Game GetGame(long gameId);
    public List<GamePlayer> GetGamePlayers(string roomId);
    public List<GamePlayer> GetGamePlayers(long gameId);
    public List<GameDart> GetGamePlayerGameDarts(string roomId);
    public List<GameDart> GetGamePlayerGameDarts(long gameId);
    public void PutGame(Game game);
    public void PutGamePlayer(GamePlayer gamePlayer);
    public void PutGameDart(GameDart gameDart);
}