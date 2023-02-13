using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Requests.Rooms.Create;
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Rooms.OnCreate;
public class CreateHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public CreateHandler() { }
    public CreateHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
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
                        nameof(CreateRoomRequest.RoomId), new AttributeValue(request.Message.RoomId)
                    }
                }
            };
            await _dynamoDb.PutItemAsync(putItemRequest);

            //using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)));

            //var postToConnectionRequest = new PostToConnectionRequest
            //{
            //    ConnectionId = request.ConnectionId,
            //    Data = stream
            //};

            //await _apiGatewayManagementApiClient.PostToConnectionAsync(postToConnectionRequest);

            return Responses.Created(JsonSerializer.Serialize(new IAmAMessage<CreateRoomRequest>
            {
                Action = "rooms/created",
                Message = request.Message
            }));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
}