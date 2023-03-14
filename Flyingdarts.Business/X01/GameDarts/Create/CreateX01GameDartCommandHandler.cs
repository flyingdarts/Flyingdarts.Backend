using Amazon.DynamoDBv2.DataModel;
using Flyingdarts.Persistance;
using Flyingdarts.Shared;
using MediatR;
using Microsoft.Extensions.Options;

namespace Flyingdarts.Business.X01;

public record CreateX01GameDartCommandHandler(IDynamoDBContext DbContext, IOptions<ApplicationOptions> ApplicationOptions) : IRequestHandler<CreateX01GameDartCommand, CreateX01GameDartResult>
{
    public async Task<CreateX01GameDartResult> Handle(CreateX01GameDartCommand request, CancellationToken cancellationToken)
    {
        var totalScore = request.Scores.Sum();

        var gameDarts = new List<GameDart>
        {
            GameDart.Create(
                request.GameId,
                request.PlayerId,
                totalScore,
                request.Game.X01!.StartingScore
            )
        };
        
        if (gameDarts == null) throw new Exception("No game darts. hmm?!");
        
        var batchWrite = DbContext.CreateBatchWrite<GameDart>(ApplicationOptions.Value.ToOperationConfig()); 
        
        batchWrite.AddPutItems(gameDarts);
        
        await batchWrite.ExecuteAsync(cancellationToken);
        
        request.GameDarts.AddRange(gameDarts);

        return CreateX01GameDartResult.Create(request.Game, request.GameDarts);
    }
}