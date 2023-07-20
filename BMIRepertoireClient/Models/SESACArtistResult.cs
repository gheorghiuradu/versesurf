using Newtonsoft.Json;

namespace LicensingService.Models
{
    public class SESACArtistResult
    {
        [JsonProperty("artist")]
        public string Artist { get; set; }

        [JsonProperty("song_num")]
        public string WorkNumber { get; set; }

        [JsonProperty("recording_title")]
        public string RecordingTitle { get; set; }
    }
}