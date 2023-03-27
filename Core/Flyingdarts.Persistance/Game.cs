namespace Flyingdarts.Persistance;
[DynamoDBTable("ApplicationTable")]
public class Game : IPrimaryKeyItem, ISortKeyItem, IAlternativeSortKeyItem
{
    [DynamoDBHashKey("PK")]
    public string PrimaryKey { get; set; }

    [DynamoDBRangeKey("SK")]
    public string SortKey { get; set; }

    [DynamoDBLocalSecondaryIndexRangeKey("LSI1")]
    public string LocalSecondaryIndexItem { get; set; }


    public long GameId { get; set; }
    public string RoomId { get; set; }
    public GameType Type { get; set; }
    public GameStatus Status { get; set; }
    public int PlayerCount { get; set; }

    public X01GameSettings X01 { get; set; }

    public DateTime CreationDate => new(GameId);

    public Game()
    {
        PrimaryKey = Constants.Game;
        GameId = DateTime.UtcNow.Ticks;
        Status = GameStatus.Qualifying;
        SortKey = $"{GameId}#{Status}"; 
        LocalSecondaryIndexItem = $"{Status}#{GameId}";
    }

    public static Game Create(int playerCount, X01GameSettings settings, string roomId)
    {
        return new Game()
        {
            Type = GameType.X01,
            PlayerCount = playerCount,
            X01 = settings,
            RoomId = roomId
        };
    }
}