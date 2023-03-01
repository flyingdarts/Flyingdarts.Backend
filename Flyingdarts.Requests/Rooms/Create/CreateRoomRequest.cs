namespace Flyingdarts.Requests.Rooms.Create;
public class CreateRoomRequest
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
}