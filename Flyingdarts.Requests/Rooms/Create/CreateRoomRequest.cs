namespace Flyingdarts.Requests.Rooms.Create;
// ReSharper disable once ClassNeverInstantiated.Global
public class CreateRoomRequest : IHaveAConnectionId
{
    public string RoomId { get; set; }
    public string? ConnectionId { get; set; }
}

public interface IHaveAConnectionId
{
    public string? ConnectionId { get; set; }
}