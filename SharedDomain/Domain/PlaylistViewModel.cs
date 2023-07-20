using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SharedDomain.Domain
{
    public class PlaylistViewModel : IPlaylistViewModel
    {
        public string Name { get; set; }
        public string Id { get; set; }
        public bool Featured { get; set; }
        public string PictureUrl { get; set; }
        public string KeyWords { get; set; }
        public string PictureHash { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum PlaylistViewModelType
    {
        Simple,
        Full
    }
}