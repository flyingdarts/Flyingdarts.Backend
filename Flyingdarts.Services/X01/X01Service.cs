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
    
    #region Puts
    
    /// <summary>
    /// Storing data
    /// </summary>
    public async Task PutGame(Game gameRequest)
    {
        var game = await _dbContext.LoadAsync<Game>(gameRequest.PrimaryKey);
        if (game != null) throw new Exception($"Game with PK {gameRequest.PrimaryKey} Already Exists");
        await _dbContext.SaveAsync(gameRequest);
    }

    public async Task PutGamePlayer(GamePlayer gamePlayerRequest)
    {
        var gamePlayer = await _dbContext.LoadAsync<GamePlayer>(gamePlayerRequest.PrimaryKey);
        if (gamePlayer == null) throw new Exception("No game player. hmm?!");
        var batchWrite = _dbContext.CreateBatchWrite<GamePlayer>(_applicationOptions.Value.ToOperationConfig());
        batchWrite.AddPutItem(gamePlayer);
        await batchWrite.ExecuteAsync(CancellationToken.None);    
    }

    public async Task PutGameDart(GameDart gameDart)
    {
        if (gameDart == null) throw new Exception("No game dart. hmm?!");
        var batchWrite = _dbContext.CreateBatchWrite<GameDart>(_applicationOptions.Value.ToOperationConfig());
        batchWrite.AddPutItem(gameDart);
        await batchWrite.ExecuteAsync(CancellationToken.None);
    }

    #endregion
    
    #region Queries
    
    /// <summary>
    /// Providing in access patterns
    /// </summary>
    public async Task<Game> GetGame(long gameId)
    {
        var result = await _dbContext.FromQueryAsync<Game>(
                GetGameQueryConfig(gameId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.Single();
    }

    public async Task<List<GamePlayer>> GetGamePlayers(long gameId)
    {
        throw new NotImplementedException();
    }

    public async Task<List<GameDart>> GetGamePlayerGameDarts(long gameId)
    {
        throw new NotImplementedException();
    }
    
    #endregion
    
    #region Query configuration

    /// <summary>
    /// Query configurations that express access patterns
    /// </summary>
    private QueryOperationConfig GetGameQueryConfig(long gameId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.Game);
        queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"{gameId}#");
        return new QueryOperationConfig { Filter = queryFilter };
    }

    private QueryOperationConfig GetGamePlayersQueryConfig(long gameId)
    {
        throw new NotImplementedException();
    }

    private QueryOperationConfig GetGamePlayerGameDartsQueryConfig(long gameId)
    {
        throw new NotImplementedException();
    }

    #endregion
}