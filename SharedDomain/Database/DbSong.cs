using Newtonsoft.Json;
using System.Collections.Generic;

namespace SharedDomain.Database
{
    [JsonObject(IsReference = true)]
    public class DbSong
    {
        public int Id { get; set; }
        public string SpotifyId { get; set; }
        public string ISRC { get; set; }
        public string Artist { get; set; }
        public string Title { get; set; }
        public string Snippet { get; set; }
        public int Plays { get; set; }
        public string PreviewUrl { get; set; }
        public int BMIWorkNumber { get; set; }
        public ICollection<DbPlaylistsSongs> DbPlaylistsSongs { get; set; }
    }
}