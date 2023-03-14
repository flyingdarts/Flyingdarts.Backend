using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Flyingdarts.Requests.Games.X01.OnScore;
using Amazon.Lambda.Core;
using Amazon.ApiGatewayManagementApi;
using Flyingdarts.Shared;

namespace Flyingdarts.Games.X01.OnScore;
public class OnScoreHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public OnScoreHandler() { }
    public OnScoreHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<X01OnScoreRequest> request, AmazonDynamoDBClient dynamoDbClient, string tableName, ILambdaContext context, AmazonApiGatewayManagementApiClient apiGatewayClient, string data, string roomId)
    {
        try
        {
            await UpdateItemAsync(dynamoDbClient, request.ConnectionId, request.Message, tableName);

            await MessageDispatcher.DispatchMessage(context, dynamoDbClient, apiGatewayClient, tableName, JsonSerializer.Serialize(request), request.Message.RoomId);

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