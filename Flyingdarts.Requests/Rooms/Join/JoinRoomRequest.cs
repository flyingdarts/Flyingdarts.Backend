namespace Flyingdarts.Requests.Rooms.Join;
public class JoinRoomRequest
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
}