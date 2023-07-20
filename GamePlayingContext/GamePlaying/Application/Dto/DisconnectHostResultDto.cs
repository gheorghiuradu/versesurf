using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class DisconnectHostResultDto
    {
        public string RoomCode { get; set; }
        public IReadOnlyList<string> GuestConnectionIds { get; set; }
    }
}