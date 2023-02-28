using System.Text.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flyingdarts.Signalling.Shared;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Rooms.Join;
using Flyingdarts.Rooms.OnJoin;

var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var innerHandler = new OnJoinHandler(tableName);
var webSocketUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
var apiGatewayClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig { ServiceURL = webSocketUrl });
// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<JoinRoomRequest>(serializer);
    return await innerHandler.Handle(socketRequest, context, dynamoDbClient, apiGatewayClient, tableName, JsonSerializer.Serialize(socketRequest.Message), socketRequest.Message.RoomId);
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();