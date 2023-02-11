using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flyingdarts.Signalling.Shared;
using System.Net;
using System.Text;
using System.Text.Json;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Games.X01.OnScore;
using Amazon.Runtime;

AmazonDynamoDBClient _dynamoDbClient = new();
string _tableName = Environment.GetEnvironmentVariable("TableName")!;
string _webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
AmazonApiGatewayManagementApiClient _apiGatewayClient = new(new AmazonApiGatewayManagementApiConfig { ServiceURL = _webSocketApiUrl });

var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    try
    {
        var socketRequest = SocketRequest<X01OnScoreRequest>.FromAPIGatewayProxyRequest(request);
        var requestDocument = Document.FromJson(JsonSerializer.Serialize(socketRequest));
        var putItemRequestAttributes = requestDocument.ToAttributeMap();
        var putItemRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = putItemRequestAttributes
        };

        await _dynamoDbClient.PutItemAsync(putItemRequest);

        var data = JsonSerializer.Serialize(new
        {
            action = "x01/on-score",
            message = socketRequest.Message
        });

        await MessageDispatcher.DispatchMessage(context, _dynamoDbClient, _apiGatewayClient, _tableName, data, socketRequest.Message.RoomId);

        return new APIGatewayProxyResponse
        {
            StatusCode = (int)HttpStatusCode.Created,
            Body = "X01 Score Input"
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