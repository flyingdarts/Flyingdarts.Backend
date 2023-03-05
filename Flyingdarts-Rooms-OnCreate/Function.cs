using Amazon.DynamoDBv2;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Rooms.Create;
using Flyingdarts.Rooms.OnCreate;
using Flyingdarts.Signalling.Shared;

var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var innerHandler = new CreateHandler(dynamoDbClient, tableName);
// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<CreateRoomRequest>(serializer);
    return await innerHandler.Handle(socketRequest);
};

await LambdaBootstrapBuilder.Create(handler, serializer)
    .Build()
    .RunAsync();