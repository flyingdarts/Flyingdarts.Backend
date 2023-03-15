// ReSharper disable once ConvertToLocalFunction
var handler = (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var connectionId = request.RequestContext.ConnectionId;

    return new APIGatewayProxyResponse
    {
        StatusCode = 200,
        Body = "Player has left the room"
    };
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
    .Build()
    .RunAsync();