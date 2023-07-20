using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Application.Commands
{
    public class RegisterGuestCommand
    {
        public string RoomCode { get; set; }
        public string GuestNick { get; set; }
        public string GuestConnectionId { get; set; }
    }
}
