namespace MusixClient
{
    using Newtonsoft.Json;
    using System;

    public partial class SnippetResponse
    {
        [JsonProperty("message")]
        public SnippetMessage Message { get; set; }
    }

    public partial class SnippetMessage
    {
        [JsonProperty("header")]
        public Header Header { get; set; }

        [JsonProperty("body")]
        public SnippetBody Body { get; set; }
    }

    public partial class SnippetBody
    {
        [JsonProperty("snippet")]
        public Snippet Snippet { get; set; }
    }

    public partial class Snippet
    {
        [JsonProperty("snippet_id")]
        public long SnippetId { get; set; }

        [JsonProperty("snippet_language")]
        public string SnippetLanguage { get; set; }

        [JsonProperty("restricted")]
        public long Restricted { get; set; }

        [JsonProperty("instrumental")]
        public long Instrumental { get; set; }

        [JsonProperty("snippet_body")]
        public string SnippetBody { get; set; }

        [JsonProperty("script_tracking_url")]
        public Uri ScriptTrackingUrl { get; set; }

        [JsonProperty("pixel_tracking_url")]
        public Uri PixelTrackingUrl { get; set; }

        [JsonProperty("html_tracking_url")]
        public Uri HtmlTrackingUrl { get; set; }

        [JsonProperty("updated_time")]
        public DateTimeOffset UpdatedTime { get; set; }
    }
}