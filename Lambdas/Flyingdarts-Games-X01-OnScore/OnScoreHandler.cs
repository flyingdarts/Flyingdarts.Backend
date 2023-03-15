namespace Flyingdarts.Games.X01.OnScore;
public class OnScoreHandler
{
    private readonly string _tableName;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly AmazonApiGatewayManagementApiClient _apiGatewayClient;
    public OnScoreHandler() { }
    public OnScoreHandler(string tableName, AmazonDynamoDBClient dynamoDbClient, AmazonApiGatewayManagementApiClient apiGatewayClient)
    {
        _tableName = tableName;
        _dynamoDbClient = dynamoDbClient;
        _apiGatewayClient = apiGatewayClient;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<X01OnScoreRequest> request, ILambdaContext context)
    {
        try
        {
            await UpdateItemAsync(_dynamoDbClient, request.ConnectionId, request.Message, _tableName);

            await MessageDispatcher.DispatchMessage(context, _dynamoDbClient, _apiGatewayClient, _tableName, JsonSerializer.Serialize(request), request.Message.RoomId);

            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
    public static async Task<bool> UpdateItemAsync(
        AmazonDynamoDBClient client,
        string connectionId,
        X01OnScoreRequest request,
        string tableName)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            ["ConnectionId"] = new AttributeValue { S = connectionId },
        };
        var updates = new Dictionary<string, AttributeValueUpdate>
        {
            [nameof(X01OnScoreRequest.Score)] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue { N = request.Score.ToString() },
            },

            [nameof(X01OnScoreRequest.Input)] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue { N = request.Input.ToString() },
            }
        };

        var updateItemRequest = new UpdateItemRequest
        {
            AttributeUpdates = updates,
            Key = key,
            TableName = tableName,
        };

        var response = await client.UpdateItemAsync(updateItemRequest);

        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
}