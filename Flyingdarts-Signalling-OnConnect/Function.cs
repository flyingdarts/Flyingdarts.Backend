using System.Net;
using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Signalling;
using Flyingdarts.Signalling.Shared;

var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var innerHandler = new ConnectHandler(dynamoDbClient, tableName);
// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    JsonDocument message = JsonDocument.Parse(request.Body);
    
    var socketRequest = new IAmAMessage<PlayerConnectedRequest>
    {
        ConnectionId = request.RequestContext.ConnectionId
    };

    if (message.RootElement.TryGetProperty("message", out var dataProperty) && !string.IsNullOrEmpty(dataProperty.GetString()))
    {
        socketRequest.Message = JsonSerializer.Deserialize<PlayerConnectedRequest>(dataProperty);
    }
    
    context.Logger.LogInformation(socketRequest.ToString());
    return await innerHandler.Handle(socketRequest);
};

await LambdaBootstrapBuilder.Create(handler, serializer)
    .Build()
    .RunAsync();
    