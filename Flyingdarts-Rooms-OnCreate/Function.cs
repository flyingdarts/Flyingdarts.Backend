using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Rooms.Create;

var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
AmazonDynamoDBClient dynamoDbClient = new();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<CreateRoomRequest>(serializer);
    try
    {
        var putItemRequest = new PutItemRequest
        {
            TableName = tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                {
                    nameof(CreateRoomRequest.ConnectionId), new AttributeValue(socketRequest.ConnectionId)
                },
                {
                    nameof(CreateRoomRequest.RoomId), new AttributeValue(socketRequest.RoomId)
                }
            }
        };
        await dynamoDbClient.PutItemAsync(putItemRequest);
        return Responses.Created("Room Created");
    }
    catch (AmazonDynamoDBException e)
    {
        return Responses.InternalServerError($"Failed to send message: {e.Message}");
    }
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, serializer)
    .Build()
    .RunAsync();