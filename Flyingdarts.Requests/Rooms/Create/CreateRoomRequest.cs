using System.Text.Json.Serialization;

namespace Flyingdarts.Requests.Rooms.Create;
public class CreateRoomRequest
{
    [JsonIgnore]
    public string? ConnectionId { get; set; }
    public string RoomId { get; set; }
}