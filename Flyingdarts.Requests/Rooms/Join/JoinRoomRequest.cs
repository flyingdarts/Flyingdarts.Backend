using Flyingdarts.Requests.Rooms.Create;
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Requests.Rooms.Join;
public class JoinRoomRequest
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
}