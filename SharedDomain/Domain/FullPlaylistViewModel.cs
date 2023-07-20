using System.Collections.Generic;

namespace SharedDomain.Domain
{
    public class FullPlaylistViewModel : IPlaylistViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public string PictureUrl { get; set; }
        public List<SongViewModel> Songs { get; set; }
        public bool Featured { get; set; }
        public string PictureHash { get; set; }
    }
}