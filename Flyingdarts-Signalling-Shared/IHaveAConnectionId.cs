using System.Text.Json.Serialization;
namespace Flyingdarts.Signalling.Shared;
public interface IHaveAConnectionId
{
    public string ConnectionId { get; set; }
}

public abstract class IAmAMessage<TMessage> : IHaveAConnectionId
{
    public string Action { get; set; } = null!;
    public TMessage Message { get; set; } = default!;

    [JsonIgnore]
    public string ConnectionId { get; set; } = null!;

    protected IAmAMessage()
    {

    }
}