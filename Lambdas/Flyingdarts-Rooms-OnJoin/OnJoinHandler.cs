namespace Flyingdarts.Rooms.OnJoin;

public class OnJoinHandler
{
    private readonly string _tableName;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly AmazonApiGatewayManagementApiClient _apiGatewayClient;
    public OnJoinHandler() { }
    public OnJoinHandler(string tableName, AmazonDynamoDBClient dynamoDbClient, AmazonApiGatewayManagementApiClient apiGatewayClient)
    {
        _tableName = tableName;
        _dynamoDbClient = dynamoDbClient;
        _apiGatewayClient = apiGatewayClient;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<JoinRoomRequest> request, ILambdaContext context)
    {
        try
        {
            await UpdateItemAsync(_dynamoDbClient, request.ConnectionId, request.Message, _tableName);

            await MessageDispatcher.DispatchMessage(context, _dynamoDbClient, _apiGatewayClient, _tableName, JsonSerializer.Serialize(request), request.Message.RoomId);

            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"failed message: {e.Message} \n Request: {JsonSerializer.Serialize(request)}");
        }
        catch (Exception e)
        {
            return Responses.InternalServerError($"Bad message: {e.Message} {e.InnerException} \n Request: {JsonSerializer.Serialize(request)}");
        }

    }
    
    public static async Task<bool> UpdateItemAsync(AmazonDynamoDBClient client, string connectionId, JoinRoomRequest request, string tableName)
    {
        var key = new Dictionary<string, AttributeValue>
        {
            ["ConnectionId"] = new AttributeValue { S = connectionId },
        };
        var updates = new Dictionary<string, AttributeValueUpdate>
        {
            [nameof(JoinRoomRequest.RoomId)] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue { S = request.RoomId },
            },

            [nameof(JoinRoomRequest.PlayerId)] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue { S = request.PlayerId },
            },

            [nameof(JoinRoomRequest.PlayerName)] = new AttributeValueUpdate
            {
                Action = AttributeAction.PUT,
                Value = new AttributeValue { S = request.PlayerName },
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