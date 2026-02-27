using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SharedDomain.Database
{
    [JsonObject(IsReference = true)]
    public class DbPlaylist
    {
        public int Id { get; set; }
        public string SpotifyId { get; set; }
        public string Name { get; set; }
        public bool Enabled { get; set; } = true;
        public bool Featured { get; set; }
        public string PictureUrl { get; set; }
        public ICollection<DbPlaylistsSongs> DbPlaylistsSongs { get; set; }
        public int Votes { get; set; }
        public int Plays { get; set; }
        public bool Free { get; set; } = false;

        public void AddSongs(IEnumerable<DbSong> dbSongs, bool enabled = true)
        {
            foreach (var song in dbSongs)
            {
                this.AddSong(song, enabled);
            }
        }

        public void AddSong(DbSong dbSong, bool enabled = true)
        {
            this.ValidateSongsLoaded();

            var existingSong = this.DbPlaylistsSongs.FirstOrDefault(ps => ps.DbSong.SpotifyId.Equals(dbSong.SpotifyId));
            if (existingSong is null)
            {
                this.DbPlaylistsSongs.Add(new DbPlaylistsSongs
                {
                    PlaylistId = this.Id,
                    DbPlaylist = this,
                    SongId = dbSong.Id,
                    DbSong = dbSong,
                    Enabled = enabled
                });
            }
        }

        public void ValidateSongsLoaded()
        {
            if (this.DbPlaylistsSongs is null)
            {
                throw new System.Exception("PlaylistsSongs not loaded, please load the collection from db before adding songs.");
            }
            else if (this.DbPlaylistsSongs.Count > 0 && this.DbPlaylistsSongs.First().DbSong is null)
            {
                throw new System.Exception("Songs from the playlist are not loaded.");
            }
        }
    }
}