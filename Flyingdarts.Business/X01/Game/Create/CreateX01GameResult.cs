using Flyingdarts.Persistance;

namespace Flyingdarts.Business.X01;
public record CreateX01GameResult(
    Game Game,
    IEnumerable<GamePlayer> GamePlayers,
    IEnumerable<GameDart> GameDarts
);