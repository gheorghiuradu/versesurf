using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class PurgeRoomResultDto
    {
        public string ActiveGameId { get; set; }
        public IReadOnlyList<string> GuestConnectionIds { get; set; }
    }
}