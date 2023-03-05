using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Requests.Signalling;
using Flyingdarts.Signalling.Shared;

public class ConnectHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public ConnectHandler()
    {
    }
    public ConnectHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<PlayerConnectedRequest> request)
    {
        try
        {
            await CreateSignallingRecord(request);
            await UpdateSignallingRecord(request);
            
            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }

    private async Task CreateSignallingRecord(IAmAMessage<PlayerConnectedRequest> request)
    {
        var ddbRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "ConnectionId", new AttributeValue{ S = request.ConnectionId }}
            }
        };

        await _dynamoDb.PutItemAsync(ddbRequest);
    }
    private async Task UpdateSignallingRecord(IAmAMessage<PlayerConnectedRequest> request)
    {
        var ddbRequest = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                { "ConnectionId", new AttributeValue{ S = request.ConnectionId }},
                { "PlayerId", new AttributeValue{ S = request.Message.PlayerId }}
            }
        };

        await _dynamoDb.PutItemAsync(ddbRequest);
    }
}