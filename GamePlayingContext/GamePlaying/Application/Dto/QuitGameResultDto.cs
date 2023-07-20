using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class QuitGameResultDto
    {
        public string GameId { get; set; }
        public IReadOnlyList<string> PlayerConnectionIds { get; set; }
    }
}