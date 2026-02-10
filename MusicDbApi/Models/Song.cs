using System;

namespace MusicDbApi.Models
{
    public class Song
    {
        public string PlaylistId { get; set; }

        public string Id { get; set; }

        public string SpotifyId { get; set; }

        public string ISRC { get; set; }

        public string Artist { get; set; }

        public string Title { get; set; }

        public bool IsExplicit { get; set; }

        public string Snippet { get; set; }

        public int Plays { get; set; }

        public bool Enabled { get; set; }

        public string FullAudioUrl { get; set; }

        public string PreviewUrl { get; set; }

        public string BmiLicenseId { get; set; }

        public string ASCAPLicenseId { get; set; }

        public string SesacLicenseId { get; set; }

        public float? StartSecond { get; set; }

        public float? EndSecond { get; set; }

        public DateTime AddedAt { get; set; }

        public DateTime ModifiedAt { get; set; }
    }
}