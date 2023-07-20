namespace MusixClient
{
    using Newtonsoft.Json;
    using System;

    public partial class TrackResponse
    {
        [JsonProperty("message")]
        public TrackReponseMessage Message { get; set; }
    }

    public partial class TrackReponseMessage
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("body")]
        public TrackResponseBody Body { get; set; }
    }

    public partial class TrackResponseBody
    {
        [JsonProperty("track")]
        public Track Track { get; set; }
    }

    public partial class Track
    {
        [JsonProperty("track_id")]
        public long TrackId { get; set; }

        [JsonProperty("track_name")]
        public string TrackName { get; set; }

        [JsonProperty("track_name_translation_list")]
        public object[] TrackNameTranslationList { get; set; }

        [JsonProperty("track_rating")]
        public long TrackRating { get; set; }

        [JsonProperty("commontrack_id")]
        public long CommontrackId { get; set; }

        [JsonProperty("instrumental")]
        public long Instrumental { get; set; }

        [JsonProperty("explicit")]
        public long Explicit { get; set; }

        [JsonProperty("has_lyrics")]
        public long HasLyrics { get; set; }

        [JsonProperty("has_subtitles")]
        public long HasSubtitles { get; set; }

        [JsonProperty("has_richsync")]
        public long HasRichsync { get; set; }

        [JsonProperty("num_favourite")]
        public long NumFavourite { get; set; }

        [JsonProperty("album_id")]
        public long AlbumId { get; set; }

        [JsonProperty("album_name")]
        public string AlbumName { get; set; }

        [JsonProperty("artist_id")]
        public long ArtistId { get; set; }

        [JsonProperty("artist_name")]
        public string ArtistName { get; set; }

        [JsonProperty("track_share_url")]
        public Uri TrackShareUrl { get; set; }

        [JsonProperty("track_edit_url")]
        public Uri TrackEditUrl { get; set; }

        [JsonProperty("restricted")]
        public long Restricted { get; set; }

        [JsonProperty("updated_time")]
        public DateTimeOffset UpdatedTime { get; set; }

        [JsonProperty("primary_genres")]
        public PrimaryGenres PrimaryGenres { get; set; }
    }

    public partial class PrimaryGenres
    {
        [JsonProperty("music_genre_list")]
        public MusicGenreList[] MusicGenreList { get; set; }
    }

    public partial class MusicGenreList
    {
        [JsonProperty("music_genre")]
        public MusicGenre MusicGenre { get; set; }
    }

    public partial class MusicGenre
    {
        [JsonProperty("music_genre_id")]
        public long MusicGenreId { get; set; }

        [JsonProperty("music_genre_parent_id")]
        public long MusicGenreParentId { get; set; }

        [JsonProperty("music_genre_name")]
        public string MusicGenreName { get; set; }

        [JsonProperty("music_genre_name_extended")]
        public string MusicGenreNameExtended { get; set; }

        [JsonProperty("music_genre_vanity")]
        public string MusicGenreVanity { get; set; }
    }

    public partial class Header
    {
        [JsonProperty("status_code")]
        public long StatusCode { get; set; }

        [JsonProperty("execute_time")]
        public double ExecuteTime { get; set; }
    }
}