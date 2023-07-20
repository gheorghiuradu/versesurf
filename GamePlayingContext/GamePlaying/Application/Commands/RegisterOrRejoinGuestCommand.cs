using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Application.Commands
{
    public class RegisterOrRejoinGuestCommand
    {
        public string RoomCode { get; set; }
        public string GuestId { get; set; }
        public string GuestNick { get; set; }
        public string ConnectionId { get; set; }
    }
}
