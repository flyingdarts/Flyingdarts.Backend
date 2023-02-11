using Flyingdarts.Requests.Rooms.Create;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flyingdarts.Requests.Rooms.Join;

public class JoinRoomRequest : SocketRequestBase
{
    public string RoomId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; }
}