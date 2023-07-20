namespace ArtManager.Models
{
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;

    public class CoverArtArchiveResponse
    {
        [JsonProperty("images")]
        public List<Image> Images { get; set; }

        [JsonProperty("release")]
        public Uri Release { get; set; }
    }

    public class Image
    {
        [JsonProperty("approved")]
        public bool Approved { get; set; }

        [JsonProperty("back")]
        public bool Back { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("edit")]
        public long Edit { get; set; }

        [JsonProperty("front")]
        public bool Front { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("image")]
        public Uri ImageImage { get; set; }

        [JsonProperty("thumbnails")]
        public Thumbnails Thumbnails { get; set; }

        [JsonProperty("types")]
        public List<string> Types { get; set; }
    }

    public class Thumbnails
    {
        [JsonProperty("250")]
        public Uri The250 { get; set; }

        [JsonProperty("500")]
        public Uri The500 { get; set; }

        [JsonProperty("1200")]
        public Uri The1200 { get; set; }

        [JsonProperty("large")]
        public Uri Large { get; set; }

        [JsonProperty("small")]
        public Uri Small { get; set; }
    }
}