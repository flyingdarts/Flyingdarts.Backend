var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var innerHandler = new OnScoreHandler(dynamoDbClient, tableName);
var webSocketUrl = Environment.GetEnvironmentVariable("WebSocketApiUrl")!;
var apiGatewayClient = new AmazonApiGatewayManagementApiClient(new AmazonApiGatewayManagementApiConfig { ServiceURL = webSocketUrl });

// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<X01OnScoreRequest>(serializer);
    return await innerHandler.Handle(socketRequest, dynamoDbClient, tableName, context, apiGatewayClient, JsonSerializer.Serialize(socketRequest), socketRequest.Message.RoomId);
};
// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();