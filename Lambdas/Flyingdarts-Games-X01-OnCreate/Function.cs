
;

var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var dynamoDbClient = new AmazonDynamoDBClient();
var x01service = new X01Service(Options.Create(new ApplicationOptions { DynamoDbTable = "ApplicationTable" }), dynamoDbClient);
var innerHandler = new X01OnCreateHandler(x01service);
// The function handler that will be called for each Lambda event
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<X01OnCreateRequest>(serializer);
    return innerHandler.Handle(socketRequest);
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();