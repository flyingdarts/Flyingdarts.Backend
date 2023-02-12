using System.Text.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<JoinRoomRequest> request, ILambdaContext context, AmazonDynamoDBClient dynamoDbClient, AmazonApiGatewayManagementApiClient apiGatewayClient, string tableName, string data, string roomId)
    {
        PutItemRequest putItemRequest = null;
        try
        {

            putItemRequest = new PutItemRequest
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
                        nameof(JoinRoomRequest.PlayerId), new AttributeValue(request.Message.PlayerId)
                    },
                    {
                        nameof(JoinRoomRequest.PlayerName), new AttributeValue(request.Message.PlayerName)
                    }
                }
            };
            await _dynamoDb.PutItemAsync(putItemRequest);

            await MessageDispatcher.DispatchMessage(context, dynamoDbClient, apiGatewayClient, tableName, JsonSerializer.Serialize(request.Message), request.Message.RoomId);

            return Responses.Created("Room Joined");
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"failed message: {e.Message} \n Request: {JsonSerializer.Serialize(request)}\nPutItemRequest{JsonSerializer.Serialize(putItemRequest)}");
        }
        catch (Exception e)
        {
            return Responses.InternalServerError($"Bad message: {e.Message} \n Request: {JsonSerializer.Serialize(request)}\nPutItemRequest{JsonSerializer.Serialize(putItemRequest)}");
        }

    }
}