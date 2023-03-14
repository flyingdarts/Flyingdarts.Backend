using Amazon.DynamoDBv2.DataModel;
using Flyingdarts.Persistance;
using Flyingdarts.Shared;
using MediatR;
using Microsoft.Extensions.Options;

namespace Flyingdarts.Business.X01;

public record CreateX01GameCommandHandler(IDynamoDBContext DbContext, IOptions<ApplicationOptions> ApplicationOptions) : IRequestHandler<CreateX01GameCommand, CreateX01GameResult>
{
    public async Task<CreateX01GameResult> Handle(CreateX01GameCommand request, CancellationToken cancellationToken)
    {
        var game = Game.Create(request.PlayerCount, new X01GameSettings(request.Sets, request.Leg, request.DoubleIn, request.DoubleOut, request.StartingScore), request.roomId);
        var players = request.PlayerIds.Select(x => GamePlayer.Create(game.GameId, x));
        var playerDarts = players.Select(x => GameDart.CreateInitial(x.GameId, x.PlayerId, request.StartingScore));

        var gameWrite = DbContext.CreateBatchWrite<Game>(ApplicationOptions.Value.ToOperationConfig()); gameWrite.AddPutItem(game);
        var gamePlayersBatch = DbContext.CreateBatchWrite<GamePlayer>(ApplicationOptions.Value.ToOperationConfig()); gamePlayersBatch.AddPutItems(players);
        var gameDartsBatch = DbContext.CreateBatchWrite<GameDart>(ApplicationOptions.Value.ToOperationConfig()); gameDartsBatch.AddPutItems(playerDarts);

        await gameWrite.Combine(gamePlayersBatch, gameDartsBatch).ExecuteAsync(cancellationToken);

        return new CreateX01GameResult(game, players, playerDarts);
    }
}