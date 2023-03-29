using Flyingdarts.Requests;

namespace Flyingdarts.Shared;

public static class APIGatewayProxyRequestExtensions
{
    public static IAmAMessage<T> To<T>(this APIGatewayProxyRequest request, ILambdaSerializer serializer) where T : class
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return null;

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(request.Body));
        var deserializedResponse = serializer.Deserialize<IAmAMessage<T>>(ms);
        deserializedResponse.ConnectionId = request.RequestContext.ConnectionId;

        return deserializedResponse;
    }

    public static IAmAMessage<DiscordIntegrationRequest> ToDiscordWebhookRequest(this APIGatewayProxyRequest request, ILambdaSerializer serializer)
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return null;

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(request.Body));
        var deserializedResponse = serializer.Deserialize<IAmAMessage<DiscordIntegrationRequest>>(ms);
        deserializedResponse.ConnectionId = request.RequestContext.ConnectionId;

        return deserializedResponse;

    }
}