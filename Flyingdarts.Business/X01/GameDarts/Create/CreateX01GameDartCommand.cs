namespace Flyingdarts.Business.X01;

public record CreateX01GameDartCommand(long GameId, Guid PlayerId, List<int> Scores) : IRequest<CreateX01GameDartResult>
{
    internal Game Game { get; set; }
    internal List<GamePlayer> GamePlayers { get; set; }
    internal List<GameDart> GameDarts { get; set; }
    
}