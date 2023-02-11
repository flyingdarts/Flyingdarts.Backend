using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.Lambda.APIGatewayEvents;

public class SocketRequest<T> where T: SocketRequestBase
{
    [JsonPropertyName("action")]
    public string Action { get; set; }
    [JsonPropertyName("message")]

    public T Message { get; set; }

    public static SocketRequest<T> FromAPIGatewayProxyRequest(APIGatewayProxyRequest request)
    {
        var socketRequest = JsonSerializer.Deserialize<SocketRequest<T>>(request.Body);
        socketRequest.Message.ConnectionId = request.RequestContext.ConnectionId;
        return socketRequest;
    }
}