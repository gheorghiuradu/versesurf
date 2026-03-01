using Newtonsoft.Json;

namespace SharedDomain.Database
{
    [JsonObject(IsReference = true)]
    public class DbPlaylistsSongs
    {
        public int Id { get; set; }
        public int PlaylistId { get; set; }
        public int SongId { get; set; }
        public DbSong DbSong { get; set; }
        public DbPlaylist DbPlaylist { get; set; }
        public bool Enabled { get; set; }
    }
}