using System.Text.Json.Serialization;

public class SocketMessage<T>
{
    [JsonPropertyName("action")]
    public string Action { get; set; }
    [JsonPropertyName("message")]

    public T Message { get; set; }
}