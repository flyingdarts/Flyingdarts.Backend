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
    private readonly string _tableName;
    public OnJoinHandler() { }
    public OnJoinHandler(string tableName)
    {
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<JoinRoomRequest> request, ILambdaContext context, AmazonDynamoDBClient dynamoDbClient, AmazonApiGatewayManagementApiClient apiGatewayClient, string tableName, string data, string roomId)
    {
        try
        {
            await UpdateItemAsync(dynamoDbClient, request.ConnectionId, request.Message);

            var players = await GetRoomPlayers(dynamoDbClient, request.Message.RoomId);

            request.Metadata = new Dictionary<string, string>
            {
                {
                    "Opponent", JsonSerializer.Serialize(players)
                }
            };
            
            await MessageDispatcher.DispatchMessage(context, dynamoDbClient, apiGatewayClient, tableName, JsonSerializer.Serialize(request), request.Message.RoomId);
    
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
    public async Task<List<string>> GetRoomPlayers(AmazonDynamoDBClient client,
        string roomId)
    {
        var returnValue = new List<string> { "Nog ni direct, ti mo ki testen xxx" };
        
        // Define query condition to search for range-keys that begin with the string "The Adventures"
        var condition = new Condition
        {
            ComparisonOperator = "BEGINS_WITH",
            AttributeValueList = new List<AttributeValue>
            {
                new AttributeValue { S = roomId }
            }
        };

        // Create the key conditions from hashKey and condition
        var keyConditions = new Dictionary<string, Condition>
        {
            // Range key condition
            {
                "RoomId",
                condition
            }
        };

            // Define marker variable
            Dictionary<string, AttributeValue> startKey = null;

            do
            {
                // Create Query request
                var request = new QueryRequest
                {
                    TableName = _tableName,
                    ExclusiveStartKey = startKey,
                    KeyConditions = keyConditions
                };

                // Issue request
                var result = await client.QueryAsync(request);

                // View all returned items
                var items = result.Items;
                foreach (var item in items)
                {
                    Console.WriteLine("Item:");
                    foreach (var keyValuePair in item)
                    {
                        Console.WriteLine("{0} : S={1}, N={2}, SS=[{3}], NS=[{4}]",
                            keyValuePair.Key,
                            keyValuePair.Value.S,
                            keyValuePair.Value.N,
                            string.Join(", ", keyValuePair.Value.SS ?? new List<string>()),
                            string.Join(", ", keyValuePair.Value.NS ?? new List<string>()));
                    }
                }

                // Set marker variable
                startKey = result.LastEvaluatedKey;
            } while (startKey != null && startKey.Count > 0);

            return returnValue;

    }
    public async Task<bool> UpdateItemAsync(
        AmazonDynamoDBClient client,
        string connectionId,
        JoinRoomRequest request)
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
            TableName = _tableName,
        };

        var response = await client.UpdateItemAsync(updateItemRequest);

        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }
}