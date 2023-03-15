namespace Flyingdarts.Business.X01;

public class CreateX01GameDartResult
{
    public long GameId { get; set; }
    public bool RoundCompleted { get; set; }
    public GameStatus GameStatus { get; set; }

    public static CreateX01GameDartResult Create(Game game, List<GameDart> latestGameDarts)
    {
        var everyPlayersDartCount = latestGameDarts.GroupBy(x => x.PlayerId).Select(x => x.Count()).ToList();
        return new CreateX01GameDartResult()
        {
            GameId = game.GameId,
            RoundCompleted = everyPlayersDartCount.Count > 0 && !everyPlayersDartCount.Distinct().Skip(1).Any(),
            GameStatus = game.Status
        };
    }
}