using Amazon.DynamoDBv2.DataModel;
using Flyingdarts.Persistance;
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Persistance;

public class GamePlayer : IPrimaryKeyItem, ISortKeyItem, IAlternativeSortKeyItem
{
    [DynamoDBHashKey("PK")]
    public string PrimaryKey { get; set; }

    [DynamoDBRangeKey("SK")]
    public string SortKey { get; set; }

    [DynamoDBLocalSecondaryIndexRangeKey("LSI1")]
    public string LocalSecondaryIndexItem { get; set; }

    public Guid PlayerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public long GameId { get; set; }

    public GamePlayer()
    {
        PrimaryKey = Constants.GamePlayer;
    }
    public static GamePlayer Create(long gameId, Guid playerId)
    {
        var now = DateTime.UtcNow;

        return new GamePlayer()
        {
            SortKey = $"{gameId}#{playerId}",
            LocalSecondaryIndexItem = $"{playerId}#{now}#{gameId}",
            CreatedAt = now,
            PlayerId = playerId,
            GameId = gameId
        };
    }
}