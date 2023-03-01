using Amazon.DynamoDBv2.DataModel;

namespace Flyingdarts.Persistance;

public interface IPrimaryKeyItem
{
    [DynamoDBRangeKey("PK")]
    public string PrimaryKey { get; set; }
}