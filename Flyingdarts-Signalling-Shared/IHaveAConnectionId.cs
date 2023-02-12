using System.Text.Json.Serialization;
namespace Flyingdarts.Signalling.Shared;
public interface IHaveAConnectionId
{
    [JsonIgnore]

    public string ConnectionId { get; set; }
}