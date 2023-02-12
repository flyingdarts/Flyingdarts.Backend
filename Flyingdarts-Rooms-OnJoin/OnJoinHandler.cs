using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Requests.Rooms.Join;
using Flyingdarts.Signalling.Shared;
namespace Flyingdarts.Rooms.OnJoin;

public class OnJoinHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public OnJoinHandler() { }
    public OnJoinHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<JoinRoomRequest> request)
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
                        nameof(JoinRoomRequest.RoomId), new AttributeValue(request.Message.RoomId)
                    },
                    {
                        nameof(JoinRoomRequest.PlayerId), new AttributeValue(request.Message.PlayerId.ToString())
                    },
                    {
                        nameof(JoinRoomRequest.PlayerName), new AttributeValue(request.Message.PlayerName)
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