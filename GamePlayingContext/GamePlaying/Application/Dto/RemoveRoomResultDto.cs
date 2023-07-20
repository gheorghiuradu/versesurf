using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class RemoveRoomResultDto
    {
        public IReadOnlyList<string> GuestConnectionIds { get; set; }
        public string ActiveGameId { get; set; }
        public string PlayFabId { get; internal set; }
    }
}