using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Flyingdarts.Persistance;
using Flyingdarts.Shared;
using Microsoft.Extensions.Options;

namespace Flyingdarts.Services.X01;

public class X01Service : IGameService
{
    private readonly IDynamoDBContext _dbContext;
    private readonly IOptions<ApplicationOptions> _applicationOptions;
    public X01Service(IDynamoDBContext dbContext, IOptions<ApplicationOptions> applicationOptions)
    {
        _dbContext = dbContext;
        _applicationOptions = applicationOptions;
    }
    private QueryOperationConfig GetGameQueryConfig(long gameId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.Game);
        queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"{gameId}#");
        return new QueryOperationConfig { Filter = queryFilter };
    }
    public Game GetGame(long gameId)
    {
        return _dbContext.FromQueryAsync<Game>(
                GetGameQueryConfig(gameId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None).Result.Single();
    }
    public async void PutGame(Game game)
    {
        if (game == null) throw new Exception("No game. hmm?!");
        var gameWrite = _dbContext.CreateBatchWrite<Game>(_applicationOptions.Value.ToOperationConfig());
        gameWrite.AddPutItem(game);
        await gameWrite.ExecuteAsync();
    }

    public async void PutGamePlayer(GamePlayer gamePlayer)
    {
        if (gamePlayer == null) throw new Exception("No game player. hmm?!");
        var batchWrite = _dbContext.CreateBatchWrite<GamePlayer>(_applicationOptions.Value.ToOperationConfig());
        batchWrite.AddPutItem(gamePlayer);
        await batchWrite.ExecuteAsync(CancellationToken.None);    
    }

    public async void PutGameDart(GameDart gameDart)
    {
        if (gameDart == null) throw new Exception("No game dart. hmm?!");
        var batchWrite = _dbContext.CreateBatchWrite<GameDart>(_applicationOptions.Value.ToOperationConfig());
        batchWrite.AddPutItem(gameDart);
        await batchWrite.ExecuteAsync(CancellationToken.None);
    }
}