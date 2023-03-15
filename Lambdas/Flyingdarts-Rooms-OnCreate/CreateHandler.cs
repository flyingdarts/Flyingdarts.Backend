using Amazon.ApiGatewayManagementApi;

namespace Flyingdarts.Rooms.OnCreate;
public class CreateHandler
{
    private readonly string _tableName;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly AmazonApiGatewayManagementApiClient _apiGatewayClient;
    public CreateHandler() { }
    public CreateHandler(string tableName, AmazonDynamoDBClient dynamoDbClient)
    {
        _tableName = tableName;
        _dynamoDbClient = dynamoDbClient;
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
            await _dynamoDbClient.PutItemAsync(putItemRequest);
            
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
        throw new NotImplementedException();
    }
}