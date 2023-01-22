using System.Text.Json.Serialization;

public class RoomJoinedRequest
{
    [JsonPropertyName("roomId")]
    public string RoomId { get; set; }
    [JsonPropertyName("playerId")] 
    public Guid PlayerId { get; set; }
    [JsonPropertyName("playerName")] 
    public string PlayerName { get; set; }
}