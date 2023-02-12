using Flyingdarts.Requests.Rooms.Create;
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Requests.Rooms.Join;
public class JoinRoomRequest : IHaveAConnectionId
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
    public string ConnectionId { get; set; }
}