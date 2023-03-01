using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Lambda.APIGatewayEvents;
using Flyingdarts.Persistance;
using Flyingdarts.Requests.Rooms.Create;
using Flyingdarts.Signalling.Shared;

namespace Flyingdarts.Rooms.OnCreate;
public class CreateHandler
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private IDynamoDBContext _dbContext;
    private readonly string _tableName;
    public CreateHandler()
    {
    }
    public CreateHandler(IAmazonDynamoDB dynamoDb, string tableName)
    {
        _dynamoDb = dynamoDb;
        _tableName = tableName;
        _dbContext = new DynamoDBContext(dynamoDb);
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<CreateRoomRequest> request)
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
                        nameof(CreateRoomRequest.RoomId), new AttributeValue(request.Message.RoomId)
                    }
                }
            };
            await _dynamoDb.PutItemAsync(putItemRequest);
            
            WritePermanentRecord(request.Message);
            
            //using var stream = new MemoryStream(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(request)));

            //var postToConnectionRequest = new PostToConnectionRequest
            //{
            //    ConnectionId = request.ConnectionId,
            //    Data = stream
            //};

            //await _apiGatewayManagementApiClient.PostToConnectionAsync(postToConnectionRequest);

            return Responses.Created(JsonSerializer.Serialize(request));
        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }

    private void WritePermanentRecord(CreateRoomRequest request)
    {
        var gameSettings = new X01GameSettings
        {
            DoubleOut = false,
            DoubleIn = false,
            Legs = 1,
            Sets = 1,
            StartingScore = 501
        };            
        var game = Game.Create(2, gameSettings, request.RoomId);

        var gamePlayer = GamePlayer.Create(game.GameId, request.PlayerId);
        var gameWrite = _dbContext.CreateBatchWrite<Game>(new DynamoDBOperationConfig { OverrideTableName = "GamesTable" });
        gameWrite.AddPutItem(game);
        var gamePlayersBatch = _dbContext.CreateBatchWrite<GamePlayer>(new DynamoDBOperationConfig { OverrideTableName = "GamesTable" }); 
        gamePlayersBatch.AddPutItems(new List<GamePlayer> { gamePlayer});
    }
}