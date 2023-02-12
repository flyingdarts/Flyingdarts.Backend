using System.Text.Json.Serialization;
namespace Flyingdarts.Signalling.Shared;


public class IAmAMessage<TMessage>
{
    public string Action { get; set; } = null!;
    public TMessage Message { get; set; } = default!;

    [JsonIgnore]
    public string ConnectionId { get; set; } = null!;

    // ReSharper disable once PublicConstructorInAbstractClass
    public IAmAMessage()
    {

    }
}