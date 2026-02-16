using System;
using System.Collections.Generic;

namespace MusicDbApi.Models
{
    public class Playlist
    {
        public string Id { get; set; }

        public string SpotifyId { get; set; }

        public string Name { get; set; }

        public bool Enabled { get; set; } = true;

        public bool Featured { get; set; }

        public string PictureUrl { get; set; }

        public virtual List<Song> Songs { get; set; }

        public int Votes { get; set; }

        public string Language { get; set; }

        public int Plays { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}