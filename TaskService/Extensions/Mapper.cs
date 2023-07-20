using MusicDbApi.Models;
using SpotifyAPI.Web.Models;
using System.Collections.Generic;
using System.Linq;

namespace TaskService.Extensions
{
    public static class Mapper
    {
        public static void MapFrom(this Playlist playlist, FullPlaylist fullPlaylist)
        {
            playlist.Name = fullPlaylist.Name;
            if (playlist.Songs is null) playlist.Songs = new List<Song>();
        }

        public static Song ToSong(this PlaylistTrack playlistTrack)
        {
            return new Song
            {
                Artist = string.Join(" & ", playlistTrack.Track.Artists.Select(a => a.Name)),
                IsExplicit = playlistTrack.Track.Explicit,
                ISRC = playlistTrack.Track.ExternalIds["isrc"],
                SpotifyId = playlistTrack.Track.Id,
                Title = playlistTrack.Track.Name
            };
        }

        public static SharedDomain.InfraEvents.MusicEvent ToDMEvent(this MusicEventDbApi.MusicEvent @event)
        => new SharedDomain.InfraEvents.MusicEvent
        {
            EventType = @event.EventType,
            PayloadJson = @event.PayloadJson,
            PayloadName = @event.PayloadName,
            PayloadType = @event.PayloadType,
            Sender = @event.Sender,
            TimeStamp = @event.TimeStamp,
        };
    }
}