using System.Text;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Flyingdarts.Requests.Rooms.Create;

public static class ExtensionMethods
{
    public static T To<T>(this APIGatewayProxyRequest request, ILambdaSerializer serializer) where T : class, IHaveAConnectionId
    {
        if (string.IsNullOrWhiteSpace(request.Body))
            return null;

        using var ms = new MemoryStream(Encoding.UTF8.GetBytes(request.Body));
        var deserializedResponse = serializer.Deserialize<T>(ms);
        deserializedResponse.ConnectionId = request.RequestContext.ConnectionId;

        return deserializedResponse;
    }
    
}
