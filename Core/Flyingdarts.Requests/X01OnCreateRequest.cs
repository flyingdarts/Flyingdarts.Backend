namespace Flyingdarts.Requests.Games.X01;
public class X01OnCreateRequest
{
    public string PlayerId { get; set; }
    public string ConnectionId { get; set; }
    public string RoomId { get; set; }
    public int StartScore { get; set; }
    public int TotalPlayers { get; set; }
    public int Legs { get; set; }
    public int Sets { get; set; }
    public bool DoubleIn { get; set; }
    public bool DoubleOut { get; set; }
}
