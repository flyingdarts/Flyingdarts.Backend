namespace Flyingdarts.Requests;

public class DiscordIntegrationRequest
{
    public string Id { get; set; }
    public string Token { get; set; }
    public int Type { get; set; }
    public int Version { get; set; }

    public static DiscordIntegrationRequest PingPong()
    {
        return new DiscordIntegrationRequest { Type = 1 };
    }
}