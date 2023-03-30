using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests;

// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var originalDiscordRequest = DiscordIntegrationRequest.FromApiGatewayProxyRequest(request, context);
    var innerHandler = new IntegrationHandler(request);
    return await innerHandler.Handle(JsonSerializer.Serialize(originalDiscordRequest));
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();

