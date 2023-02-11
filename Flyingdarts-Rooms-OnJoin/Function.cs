using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Runtime;
using Flyingdarts.Signalling.Shared;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Rooms.Join;
using Flyingdarts.Requests.Rooms.Create;

var _dynamoDbClient = new AmazonDynamoDBClient();
var _tableName = Environment.GetEnvironmentVariable("TableName")!;
var _webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
AmazonApiGatewayManagementApiClient _apiGatewayClient = new(new AmazonApiGatewayManagementApiConfig { ServiceURL = _webSocketApiUrl });

var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{

    try
    {
        var socketRequest = SocketRequest<JoinRoomRequest>.FromAPIGatewayProxyRequest(request);
        var requestDocument = Document.FromJson(JsonSerializer.Serialize(socketRequest));
        var putItemRequestAttributes = requestDocument.ToAttributeMap();
        var putItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = putItemRequestAttributes
        };

        await _dynamoDbClient.PutItemAsync(putItemRequest);

        // List all of the current connections. In a more advanced use case the table could be used to grab a group of connection ids for a chat group.
        var scanRequest = new ScanRequest
        {
            TableName = _tableName,
            ProjectionExpression = $"{Fields.ConnectionId},{Fields.RoomId},{Fields.PlayerId}"
        };

        var scanResponse = await _dynamoDbClient.ScanAsync(scanRequest);

        if (!scanResponse.Items.Any())
            return new APIGatewayProxyResponse
            {
                StatusCode = 200
            };

        var data = JsonSerializer.Serialize(new
        {
            action = "room/on-join",
            message = socketRequest.Message
        });

        var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));

        var connectedClientsInRoom = scanResponse.Items.Where(x => x[Fields.RoomId].S == socketRequest.Message.RoomId);

        // Loop through all of the connections and broadcast the message out to the connections.
        var count = 0;
        foreach (var item in connectedClientsInRoom)
        {
            var postConnectionRequest = new PostToConnectionRequest
            {
                ConnectionId = item[Fields.ConnectionId].S,
                Data = stream
            };

            try
            {
                context.Logger.LogInformation($"Post to connection {count}: {postConnectionRequest.ConnectionId}");
                stream.Position = 0;
                await _apiGatewayClient.PostToConnectionAsync(postConnectionRequest);
                count++;
            }
            catch (AmazonServiceException e)
            {
                // API Gateway returns a status of 410 GONE then the connection is no
                // longer available. If this happens, delete the identifier
                // from our DynamoDB table.
                if (e.StatusCode == HttpStatusCode.Gone)
                {
                    var ddbDeleteRequest = new DeleteItemRequest
                    {
                        TableName = _tableName,
                        Key = new Dictionary<string, AttributeValue>
                        {
                            {Fields.ConnectionId, new AttributeValue {S = postConnectionRequest.ConnectionId}}
                        }
                    };

                    context.Logger.LogInformation($"Deleting gone connection: {postConnectionRequest.ConnectionId}");
                    await _dynamoDbClient.DeleteItemAsync(ddbDeleteRequest);
                }
                else
                {
                    context.Logger.LogInformation($"Error posting message to {postConnectionRequest.ConnectionId}: {e.Message}");
                    context.Logger.LogInformation(e.StackTrace);
                }
            }
        }

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Created,
            Body = "Room Joined"
        };
    }
    catch (Exception e)
    {
        context.Logger.LogInformation("Error disconnecting: " + e.Message);
        context.Logger.LogInformation(e.StackTrace);
        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.InternalServerError,
            Body = $"Failed to send message: {e.Message}"
        };
    }
};
// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();