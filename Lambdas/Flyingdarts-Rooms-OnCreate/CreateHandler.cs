namespace Flyingdarts.Rooms.OnCreate;
public class CreateHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private IDynamoDBContext _dbContext;
    private readonly string _tableName;
    public CreateHandler()
    {
    }
    public CreateHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
        _dbContext = new DynamoDBContext(dynamoDb);
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<CreateRoomRequest> request)
    {
        try
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {
                        nameof(request.ConnectionId), new AttributeValue(request.ConnectionId)
                    },
                    {
                        nameof(request.Message.PlayerId), new AttributeValue(request.Message.PlayerId.ToString().ToLower())
                    }
                }
            };
            await _dynamoDb.PutItemAsync(putItemRequest);
            
            await WritePermanentRecord(request.Message);
            
            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }

    private async Task WritePermanentRecord(CreateRoomRequest request)
    {
        var gameSettings = new X01GameSettings
        {
            DoubleOut = false,
            DoubleIn = false,
            Legs = 1,
            Sets = 1,
            StartingScore = 501
        };            
        var game = Game.Create(2, gameSettings, request.RoomId);

        var gamePlayer = GamePlayer.Create(game.GameId, request.PlayerId);
        var gameWrite = _dbContext.CreateBatchWrite<Game>(new DynamoDBOperationConfig { OverrideTableName = "ApplicationTable" });
        gameWrite.AddPutItem(game);
        var gamePlayersBatch = _dbContext.CreateBatchWrite<GamePlayer>(new DynamoDBOperationConfig { OverrideTableName = "ApplicationTable" }); 
        gamePlayersBatch.AddPutItems(new List<GamePlayer> { gamePlayer});

        await gameWrite.ExecuteAsync();
        await gamePlayersBatch.ExecuteAsync();
    }
}