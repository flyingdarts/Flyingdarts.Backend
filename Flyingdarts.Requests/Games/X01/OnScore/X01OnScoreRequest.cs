namespace Flyingdarts.Requests.Games.X01.OnScore;
public class X01OnScoreRequest : SocketRequestBase
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
    public int Score { get; set; }
    public int Input { get; set; }
}

