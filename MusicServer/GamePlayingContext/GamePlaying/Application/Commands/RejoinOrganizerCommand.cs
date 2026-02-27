using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Application.Commands
{
    public class RejoinOrganizerCommand
    {
        public string RoomCode { get; set; }
        public string ConnectionId { get; set; }
    }
}
