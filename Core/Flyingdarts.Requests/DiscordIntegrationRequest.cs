using System.Text.Json.Serialization;

namespace Flyingdarts.Requests;

public class DiscordIntegrationRequest
{
    public string Id { get; set; }
    [JsonPropertyName("application_id")]
    public string ApplicationId { get; set; }
    public string Token { get; set; }
    public int Type { get; set; }
    public DiscordUser User { get; set; }
    public int Version { get; set; }

    public class DiscordUser
    {
        [JsonPropertyName("avatar")]
        public string Avatar { get; set; }
        [JsonPropertyName("avatar_decoration")]
        public object VatarDecoration { get; set; }
        [JsonPropertyName("discriminator")]
        public string Discriminator { get; set; }
        [JsonPropertyName("display_name")]
        public object DisplayName { get; set; }
        [JsonPropertyName("global_name")]
        public object GlobalName { get; set; }
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("public_flags")]
        public int PublicFlags { get; set; }
        [JsonPropertyName("username")]
        public string Username { get; set; }
    }
    public static DiscordIntegrationRequest PingPong()
    {
        return new DiscordIntegrationRequest { Type = 1 };
    }
}

