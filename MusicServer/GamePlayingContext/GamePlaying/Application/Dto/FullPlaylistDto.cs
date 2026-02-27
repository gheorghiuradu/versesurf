using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class FullPlaylistDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PictureUrl { get; set; }
        public bool Featured { get; set; }
        public IEnumerable<FullSongDto> Songs { get; set; }
        public string PictureHash { get; set; }
    }
}