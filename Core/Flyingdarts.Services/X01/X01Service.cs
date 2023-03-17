using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;

namespace Flyingdarts.Services.X01;

public class X01Service : IGameService
{
    private readonly IDynamoDBContext _dbContext;
    private readonly IOptions<ApplicationOptions> _applicationOptions;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    public X01Service(IDynamoDBContext dbContext, IOptions<ApplicationOptions> applicationOptions, AmazonDynamoDBClient dynamoDbClient)
    {
        _dbContext = dbContext;
        _applicationOptions = applicationOptions;
        _dynamoDbClient = dynamoDbClient;
    }
    public async Task<bool> DoesItemExist(string pk, string sk, string lsiName, string lsiValue, string tableName, AmazonDynamoDBClient client)
    {
        // Create a QueryRequest object to check if the item exists in the table
        var request = new QueryRequest
        {
            TableName = tableName,
            KeyConditionExpression = "#pk = :pk and #sk = :sk",
            ExpressionAttributeNames = new Dictionary<string, string>
            {
                { "#pk", "PK" },
                { "#sk", "SK" }
            },
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                { ":pk", new AttributeValue { S = pk } },
                { ":sk", new AttributeValue { S = sk } }
            }
        };

        // If an LSI name and value are provided, add them to the request
        if (!string.IsNullOrEmpty(lsiName) && !string.IsNullOrEmpty(lsiValue))
        {
            request.IndexName = lsiName;
            request.KeyConditionExpression += " and #lsi = :lsi";
            request.ExpressionAttributeNames.Add("#lsi", lsiName);
            request.ExpressionAttributeValues.Add(":lsi", new AttributeValue { S = lsiValue });
        }

        // Execute the query and check if any items were returned
        var response = await client.QueryAsync(request);
        return response.Items.Count > 0;
    }
    
    public async Task PutGame(Game request)
    {
        if (await DoesItemExist(request.PrimaryKey, request.SortKey, "LSI1", request.LocalSecondaryIndexItem, "ApplicationTable", _dynamoDbClient))
            throw new Exception($"Game with PK {request.PrimaryKey} SK {request.SortKey} LSI1 {request.LocalSecondaryIndexItem} Already Exists");
        await _dbContext.SaveAsync(request);
    }

    public async Task PutGamePlayer(GamePlayer request)
    {
        if (await DoesItemExist(request.PrimaryKey, request.SortKey, "LSI1", request.LocalSecondaryIndexItem, "ApplicationTable", _dynamoDbClient))
            throw new Exception($"GamePlayer with PK {request.PrimaryKey} SK {request.SortKey} LSI1 {request.LocalSecondaryIndexItem} Already Exists");
        await _dbContext.SaveAsync(request);
    }

    public async Task PutGameDart(GameDart request)
    {
        if (await DoesItemExist(request.PrimaryKey, request.SortKey, "LSI1", request.LocalSecondaryIndexItem, "ApplicationTable", _dynamoDbClient))
            throw new Exception($"GameDart with PK {request.PrimaryKey} SK {request.SortKey} LSI1 {request.LocalSecondaryIndexItem} Already Exists");
        await _dbContext.SaveAsync(request);
    }

    public async Task<Game> GetGame(long gameId)
    {
        var result = await _dbContext.FromQueryAsync<Game>(
                GetGameQueryConfig(gameId),
                _applicationOptions.Value.ToOperationConfig())
            .GetRemainingAsync(CancellationToken.None);
        return result.Single();
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
    
    private QueryOperationConfig GetGamePlayerGameDartsQueryConfig(long gameId)
    {
        var queryFilter = new QueryFilter("PK", QueryOperator.Equal, Constants.GameDart);
        return new QueryOperationConfig { Filter = queryFilter };
    }
}