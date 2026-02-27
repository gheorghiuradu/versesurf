using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class KickGuestsResultDto
    {
        public IReadOnlyList<string> GuestConnectionIds { get; set; }
    }
}