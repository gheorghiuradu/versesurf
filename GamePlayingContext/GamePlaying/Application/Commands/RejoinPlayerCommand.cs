using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Application.Commands
{
    public class RejoinPlayerCommand
    {
        public string RoomCode { get; set; }
        public string PlayerId { get; set; }
    }
}
