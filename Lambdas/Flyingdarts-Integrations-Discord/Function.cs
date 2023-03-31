using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using Flyingdarts.Requests;

// ReSharper disable once ConvertToLocalFunction
var handler = async (APIGatewayProxyRequest request, ILambdaContext context) =>
{
    var originalDiscordRequest = DiscordIntegrationRequest.FromApiGatewayProxyRequest(request);
    var innerHandler = new IntegrationHandler(request);
    return await innerHandler.Handle(originalDiscordRequest.RawBody, context);
};

await LambdaBootstrapBuilder.Create(handler, new DefaultLambdaJsonSerializer())
        .Build()
        .RunAsync();

