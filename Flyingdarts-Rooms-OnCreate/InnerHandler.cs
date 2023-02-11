using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Requests.Rooms.Create;

public class InnerHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;

    public InnerHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(CreateRoomRequest request)
    {
        try
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {
                        nameof(CreateRoomRequest.ConnectionId), new AttributeValue(request.ConnectionId)
                    },
                    {
                        nameof(CreateRoomRequest.RoomId), new AttributeValue(request.RoomId)
                    }
                }
            };
            await _dynamoDb.PutItemAsync(putItemRequest);
            return Responses.Created("Room Created");
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
}