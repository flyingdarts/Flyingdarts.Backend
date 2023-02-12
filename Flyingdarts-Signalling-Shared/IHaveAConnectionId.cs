using System.Text.Json.Serialization;
namespace Flyingdarts.Signalling.Shared;
public interface IHaveAConnectionId
{
    [JsonIgnore]

    public string ConnectionId { get; set; }
}

public interface IAmAMessage<TMessage> : IHaveAConnectionId
{
    public string Action { get; set; }
    public TMessage Message { get; set; }
}