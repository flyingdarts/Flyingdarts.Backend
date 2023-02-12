
namespace Flyingdarts.Requests.Games.X01.OnScore;
public class X01OnScoreRequest
{
    public string RoomId { get; set; }
    public string PlayerId { get; set; }
    public int Score { get; set; }
    public int Input { get; set; }
}