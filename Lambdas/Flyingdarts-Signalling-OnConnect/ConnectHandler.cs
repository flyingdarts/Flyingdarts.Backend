public class ConnectHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public ConnectHandler()
    {
    }
    public ConnectHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<PlayerConnectedRequest> request)
    {
        try
        {
            if (request != null)
                await CreateSignallingRecord(request.ConnectionId);

            if (request?.Message != null && !string.IsNullOrEmpty(request.Message.PlayerId))
                await UpdateSignallingRecord(request.ConnectionId, request.Message.PlayerId);
            
            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }

    private async Task CreateSignallingRecord(string connectionId)
    {
        var ddbRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "ConnectionId", new AttributeValue{ S = connectionId }}
            }
        };

        await _dynamoDb.PutItemAsync(ddbRequest);
    }
    private async Task UpdateSignallingRecord(string connectionId, string playerId)
    {
        var ddbRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "ConnectionId", new AttributeValue{ S = connectionId }},
                { "PlayerId", new AttributeValue{ S = playerId }}
            }
        };

        await _dynamoDb.PutItemAsync(ddbRequest);
    }
}