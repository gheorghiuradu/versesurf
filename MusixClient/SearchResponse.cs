namespace MusixClient
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;
    using System.Globalization;

    public partial class SearchResponse
    {
        [JsonProperty("message")]
        public SearchResponseMessage Message { get; set; }
    }

    public partial class SearchResponseMessage
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("body")]
        public SearchResponseBody Body { get; set; }
    }

    public partial class SearchResponseBody
    {
        [JsonProperty("track_list")]
        public Track[] TrackList { get; set; }
    }

    public partial class TrackList
    {
        [JsonProperty("track")]
        public Track Track { get; set; }
    }

    public partial class TrackNameTranslationList
    {
        [JsonProperty("track_name_translation")]
        public TrackNameTranslation TrackNameTranslation { get; set; }
    }

    public partial class TrackNameTranslation
    {
        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("translation")]
        public string Translation { get; set; }
    }

    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}