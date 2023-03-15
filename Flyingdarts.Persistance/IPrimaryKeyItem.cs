namespace Flyingdarts.Persistance;

public interface IPrimaryKeyItem
{
    [DynamoDBRangeKey("PK")]
    public string PrimaryKey { get; set; }
}