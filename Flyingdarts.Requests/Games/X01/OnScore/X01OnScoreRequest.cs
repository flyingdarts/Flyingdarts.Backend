
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Requests.Games.X01.OnScore;
public class X01OnScoreRequest : IHaveAConnectionId
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public int Score { get; set; }
    public int Input { get; set; }
    public string ConnectionId { get; set; }
}