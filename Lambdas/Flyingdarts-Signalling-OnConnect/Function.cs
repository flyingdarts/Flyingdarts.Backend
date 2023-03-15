var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var tableName = Environment.GetEnvironmentVariable("TableName")!;
var innerHandler = new ConnectHandler(dynamoDbClient, tableName);
// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = new IAmAMessage<PlayerConnectedRequest>
    {
        ConnectionId = request.RequestContext.ConnectionId
    };
    
    if (request.Body is not null)
    {
        var message = JsonDocument.Parse(request.Body);
        if (message.RootElement.TryGetProperty("message", out var dataProperty) && !string.IsNullOrEmpty(dataProperty.GetString()))
        {
            socketRequest.Message = JsonSerializer.Deserialize<PlayerConnectedRequest>(dataProperty);
        }
    }
    
    context.Logger.LogInformation(socketRequest.ToString());
    return await innerHandler.Handle(socketRequest);
};

await LambdaBootstrapBuilder.Create(handler, serializer)
    .Build()
    .RunAsync();
    