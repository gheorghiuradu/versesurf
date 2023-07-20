using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class NewGameStartedDto
    {
        public string GameId { get; set; }
        public IEnumerable<PlaylistDto> Playlists { get; set; }
    }
}