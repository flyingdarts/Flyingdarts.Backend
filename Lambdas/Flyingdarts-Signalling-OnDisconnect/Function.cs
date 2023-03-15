var DynamoDbClient = new AmazonDynamoDBClient();
var TableName = Environment.GetEnvironmentVariable("TableName")!;
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var connectionId = request.RequestContext.ConnectionId;
    context.Logger.LogInformation($"ConnectionId: {connectionId}");

    var ddbRequest = new DeleteItemRequest
    {
        TableName = TableName,
        Key = new Dictionary<string, AttributeValue>
        {
            { "ConnectionId", new AttributeValue { S = connectionId } }
        }
    };

    await DynamoDbClient.DeleteItemAsync(ddbRequest);

    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = "Disconnected."
    };
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();