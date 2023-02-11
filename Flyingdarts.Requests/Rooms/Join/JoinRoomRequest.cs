using System.Text.Json.Serialization;

namespace Flyingdarts.Requests.Rooms.Join;
public class JoinRoomRequest
{
    [JsonIgnore]
    public string? ConnectionId { get; set; }
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
}