using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests.Signalling;
using Flyingdarts.Signalling.Shared;

var DynamoDbClient = new AmazonDynamoDBClient();
var TableName = Environment.GetEnvironmentVariable("TableName")!;
var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
// The function handler that will be called for each Lambda event
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<PlayerConnectedRequest>(serializer);
    var ddbRequest = new PutItemRequest
    {
        TableName = TableName,
        Item = new Dictionary<string, AttributeValue>
        {
            { "ConnectionId", new AttributeValue{ S = socketRequest.ConnectionId }},
            { "PlayerId", new AttributeValue{ S = socketRequest.Message.PlayerId }}
        }
    };

    await DynamoDbClient.PutItemAsync(ddbRequest);

    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = "Connected"
    };
};

// Build the Lambda runtime client passing in the handler to call for each
// event and the JSON serializer to use for translating Lambda JSON documents
// to .NET types.
await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();