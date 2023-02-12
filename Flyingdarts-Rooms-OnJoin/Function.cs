using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flyingdarts.Signalling.Shared;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Rooms.Join;
using Flyingdarts.Rooms.OnJoin;

var _serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var _dynamoDbClient = new AmazonDynamoDBClient();
var _tableName = Environment.GetEnvironmentVariable("TableName")!;
var _webSocketApiUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
AmazonApiGatewayManagementApiClient _apiGatewayClient = new(new AmazonApiGatewayManagementApiConfig { ServiceURL = _webSocketApiUrl });
var innerHandler = new InnerHandler(_dynamoDbClient, _tableName);
// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<JoinRoomRequest>(_serializer);

    // await MessageDispatcher.DispatchMessage(context, _dynamoDbClient, _apiGatewayClient, _tableName, data, socketRequest.Message.RoomId);

    return await innerHandler.Handle(socketRequest);
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();