using MusicDbApi.Models;
using SpotifyAPI.Web.Models;
using System.Linq;

namespace BackOffice.Web.Data
{
    public static class Mapper
    {
        public static void MapFromSpotify(this Song song, FullTrack fullTrack)
        {
            song.Artist = string.Join(" & ", fullTrack.Artists.Select(a => a.Name));
            song.IsExplicit = fullTrack.Explicit;
            song.ISRC = fullTrack.ExternalIds["isrc"];
            song.SpotifyId = fullTrack.Id;
            song.Title = fullTrack.Name;
        }
    }
}