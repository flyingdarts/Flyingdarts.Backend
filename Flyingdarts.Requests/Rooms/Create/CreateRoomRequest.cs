namespace Flyingdarts.Requests.Rooms.Create;
public class CreateRoomRequest : SocketRequestBase
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
}