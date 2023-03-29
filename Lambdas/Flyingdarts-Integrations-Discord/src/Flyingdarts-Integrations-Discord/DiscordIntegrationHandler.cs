namespace Flyingdarts.Integrations.Discord;

public class DiscordIntegrationHandler
{
    private readonly string _tableName;
    private readonly AmazonDynamoDBClient _dynamoDbClient;
    private readonly AmazonApiGatewayManagementApiClient _apiGatewayClient;
    public DiscordIntegrationHandler() { }
    public DiscordIntegrationHandler(string tableName, AmazonDynamoDBClient dynamoDbClient, AmazonApiGatewayManagementApiClient apiGatewayClient)
    {
        _tableName = tableName;
        _dynamoDbClient = dynamoDbClient;
        _apiGatewayClient = apiGatewayClient;
    }

    public async Task<APIGatewayProxyResponse> Handle(IAmAMessage<DiscordIntegrationRequest> request, ILambdaContext context)
    {
        try
        {
            if (request.Message.Type != 1)
                return Responses.InternalServerError("Ping pong failure");
            return Responses.Created(JsonSerializer.Serialize(DiscordIntegrationRequest.PingPong));
        }
        catch (Exception e)
        {
            return Responses.InternalServerError($"Failed to send message: {e.Message}");
        }
    }
}
