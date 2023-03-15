namespace Flyingdarts.Persistance;

public interface ISortKeyItem
{
    [DynamoDBRangeKey("SK")] 
    public string SortKey { get; set; }
}