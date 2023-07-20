using System.Collections.Generic;

namespace GamePlaying.Application.Commands
{
    public class KickGuestsCommand
    {
        public string RoomCode { get; set; }
        public string HostConnectionId { get; set; }
        public IEnumerable<string> GuestIds { get; set; }
    }
}