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
        try
        {
            await UpdateItemAsync(dynamoDbClient, request.ConnectionId, request.Message, tableName);

            // await MessageDispatcher.DispatchMessage(context, dynamoDbClient, apiGatewayClient, tableName, JsonSerializer.Serialize(request.Message), request.Message.RoomId);

            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"failed message: {e.Message} \n Request: {JsonSerializer.Serialize(request)}");
        }
        catch (Exception e)
        {
            return Responses.InternalServerError($"Bad message: {e.Message} \n Request: {JsonSerializer.Serialize(request)}");
        }

    }
    public static async Task<bool> UpdateItemAsync(
        AmazonDynamoDBClient client,
        string connectionId,
        JoinRoomRequest request,
        string tableName)
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