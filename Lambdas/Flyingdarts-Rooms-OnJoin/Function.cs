var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var webSocketUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
var apiGatewayClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig { ServiceURL = webSocketUrl });
var innerHandler = new OnJoinHandler(tableName, dynamoDbClient, apiGatewayClient);
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<JoinRoomRequest>(serializer);
    return await innerHandler.Handle(socketRequest, context);
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();