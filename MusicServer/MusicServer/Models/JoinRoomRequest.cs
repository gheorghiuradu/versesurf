using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicServer.Models
{
    public class JoinRoomRequest
    {
        public string GuestId { get; set; }
        public string RoomCode { get; set; }
        public string GuestNick { get; set; }
    }
}
