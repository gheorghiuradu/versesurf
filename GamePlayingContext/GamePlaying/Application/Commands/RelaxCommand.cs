using System.Collections.Generic;

namespace GamePlaying.Application.Commands
{
    public class RelaxCommand
    {
        public string RoomCode { get; set; }
        public IEnumerable<string> PlayerOrGuestIds { get; set; }
    }
}