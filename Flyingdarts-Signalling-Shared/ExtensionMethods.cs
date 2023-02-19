using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
namespace Flyingdarts.Signalling.Shared;

public static class ExtensionMethods
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

}
