using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Rooms.Create;

AmazonDynamoDBClient _dynamoDbClient = new();
string _tableName = Environment.GetEnvironmentVariable("TableName")!;
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    try
    {
        var socketRequest = JsonSerializer.Deserialize<CreateRoomRequest>(request.Body);
        socketRequest!.ConnectionId = request.RequestContext.ConnectionId;
        var requestDocument = Document.FromJson(JsonSerializer.Serialize(socketRequest));
        var putItemRequestAttributes = requestDocument.ToAttributeMap();
        var putItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = putItemRequestAttributes
        };

        await _dynamoDbClient.PutItemAsync(putItemRequest);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Created,
            Body = "Room Created"
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
