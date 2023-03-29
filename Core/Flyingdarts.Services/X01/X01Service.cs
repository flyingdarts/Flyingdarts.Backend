using Amazon.DynamoDBv2;

namespace Flyingdarts.Services.X01;

public class X01Service : IGameService
{
    private readonly IDynamoDBContext _dbContext;
    private readonly IOptions<ApplicationOptions> _applicationOptions;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    public X01Service(IOptions<ApplicationOptions> applicationOptions, AmazonDynamoDBClient dynamoDbClient)
    {
        _applicationOptions = applicationOptions;
        _dynamoDbClient = dynamoDbClient;
        _dbContext = new DynamoDBContext(_dynamoDbClient);
    }

    public async Task PutGame(Game request)
    {
        var game = await GetGame(request.GameId);
        if (game != null)
            throw new Exception($"Game with PK {request.PrimaryKey} SK {request.SortKey} LSI1 {request.LocalSecondaryIndexItem} Already Exists");
        await _dbContext.SaveAsync(request);
    }

    public async Task PutGamePlayer(GamePlayer request)
    {
        var gamePlayer = await GetGamePlayer(request.PlayerId);
        if (gamePlayer is not null)
            throw new Exception($"GamePlayer with PK {request.PrimaryKey} SK {request.SortKey} LSI1 {request.LocalSecondaryIndexItem} Already Exists");
        await _dbContext.SaveAsync(request);
    }

    public async Task PutGameDart(GameDart request)
    {
        var gameDarts = await GetGamePlayerGameDarts(request.GameId);
        if (gameDarts.Any(x=>x.SortKey == request.SortKey))
            throw new Exception($"GameDart with PK {request.PrimaryKey} SK {request.SortKey} LSI1 {request.LocalSecondaryIndexItem} Already Exists");
        await _dbContext.SaveAsync(request);
    }

    public async Task<Game> GetGame(long gameId)
    {
        var result = await _dbContext.FromQueryAsync<Game>(
                GetGameQueryConfig(gameId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.SingleOrDefault();
    }
    
    public async Task<List<Game>> GetLatestGames()
    {
        var result = await _dbContext.FromQueryAsync<Game>(
                GetLatestGamesQueryConfig(),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.Take(10).ToList();
    }

    public async Task<List<GamePlayer>> GetGamePlayers(long gameId)
    {
        var result = await _dbContext.FromQueryAsync<GamePlayer>(
                GetGamePlayersQueryConfig(gameId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.Take(10).ToList();
    }

    public async Task<GamePlayer> GetGamePlayer(Guid playerId)
    {
        var result = await _dbContext.FromQueryAsync<GamePlayer>(
                GetGamePlayerQueryConfig(playerId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.SingleOrDefault();
    }

    public async Task<List<GameDart>> GetGamePlayerGameDarts(long gameId)
    {
        var result = await _dbContext.FromQueryAsync<GameDart>(
                GetGamePlayerGameDartsQueryConfig(gameId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.Take(10).ToList();
    }
   
    private QueryOperationConfig GetGameQueryConfig(long gameId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.Game);
        queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"{gameId}#");
        return new QueryOperationConfig { Filter = queryFilter };
    }

    private QueryOperationConfig GetLatestGamesQueryConfig()
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.Game);
        return new QueryOperationConfig { Filter = queryFilter };
    }
    
    private QueryOperationConfig GetGamePlayersQueryConfig(long gameId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.GamePlayer);
        return new QueryOperationConfig { Filter = queryFilter };
    }

    private QueryOperationConfig GetGamePlayerQueryConfig(Guid gamePlayerId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.GamePlayer);
        queryFilter.AddCondition("LocalSecondaryIndexItem", QueryOperator.BeginsWith, gamePlayerId);
        return new QueryOperationConfig { Filter = queryFilter };
    }
    
    private QueryOperationConfig GetGamePlayerGameDartsQueryConfig(long gameId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.GameDart);
        return new QueryOperationConfig { Filter = queryFilter };
    }
}