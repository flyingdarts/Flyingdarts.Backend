using System.Text.Json.Serialization;

public class X01ScoreRequest
{
    [JsonPropertyName("roomId")]
    public string RoomId { get; set; }
    [JsonPropertyName("playerId")]
    public Guid PlayerId { get; set; }
    [JsonPropertyName("score")]
    public int Score { get; set; }
    [JsonPropertyName("input")]
    public int Input { get; set; }
}