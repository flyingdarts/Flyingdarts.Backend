namespace Flyingdarts.Games.X01.OnCreate;
public class X01OnCreateHandler
{
    private readonly X01Service _x01Service;
    public X01OnCreateHandler() { }
    public X01OnCreateHandler(X01Service x01Service)
    {
        _x01Service = x01Service;
    }
    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<X01OnCreateRequest> request)
    {
        try
        {
            if (request.Message is null) return Responses.Created("Message null");

            var msg = request.Message;
            var gameSettings = new X01GameSettings
            {
                DoubleIn = msg.DoubleIn,
                DoubleOut = msg.DoubleOut,
                Legs = msg.Legs,
                Sets = msg.Sets,
                StartingScore = msg.StartScore
            };
            var game = Game.Create(msg.TotalPlayers, gameSettings, msg.RoomId);
    
            await _x01Service.PutGame(game);
            await _x01Service.PutGamePlayer(GamePlayer.Create(game.GameId, Guid.Parse(msg.PlayerId)));
                
            return Responses.Created(JsonSerializer.Serialize(request));

        }
        catch (AmazonDynamoDBException e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
}