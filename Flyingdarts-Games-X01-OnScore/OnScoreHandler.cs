using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using System.Text.Json;
using Flyingdarts.Requests.Games.X01.OnScore;
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Games.X01.OnScore;
public class OnScoreHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    public OnScoreHandler() {}
    public OnScoreHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<X01OnScoreRequest> request)
    {
        try
        {
            var putItemRequest = new PutItemRequest
            {
                TableName = _tableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    {
                        nameof(request.ConnectionId), new AttributeValue(request.ConnectionId)
                    },
                    {
                        nameof(X01OnScoreRequest.RoomId), new AttributeValue(request.Message.RoomId)
                    }
                }
            };
            await _dynamoDb.PutItemAsync(putItemRequest);
            
            var data = JsonSerializer.Serialize(new
            {
                action = "x01/on-score",
                message = request.Message
            });

            //await MessageDispatcher.DispatchMessage(_context, _dynamoDbClient, _apiGatewayClient, _tableName, data, socketRequest.RoomId);

            return Responses.Created("Room Created");
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
}