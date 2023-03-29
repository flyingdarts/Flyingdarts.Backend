

var serializer = new DefaultLambdaJsonSerializer(x => x.PropertyNameCaseInsensitive = true);
var clientConfig = new DiscordSocketConfig { MessageCacheSize = 50 };
var client = new DiscordSocketClient(clientConfig);
var innerHandler = new DiscordIntegrationHandler(client);
// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var socketRequest = request.To<DiscordIntegrationRequest>(serializer);
    return await innerHandler.Handle(socketRequest, context);
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();