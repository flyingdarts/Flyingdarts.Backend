using System.Text.Json.Serialization;
namespace Flyingdarts.Signalling.Shared;


public class IAmAMessage<TMessage>
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = null!;

    [JsonPropertyName("message")]
    public TMessage Message { get; set; } = default!;

    [JsonPropertyName("metadata")]
    public Dictionary<string, string>? Metadata { get; set; } = null!;

    [JsonIgnore]
    public string ConnectionId { get; set; } = null!;



    // ReSharper disable once PublicConstructorInAbstractClass
    public IAmAMessage()
    {

    }
}