namespace Flyingdarts.Persistance;

public interface IAlternativeSortKeyItem
{
    [DynamoDBRangeKey("LSI1")] 
    public string LocalSecondaryIndexItem { get; set; }
}