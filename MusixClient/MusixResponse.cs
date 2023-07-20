namespace MusixClient
{
    using Newtonsoft.Json;

    public class MusixResponse
    {
        [JsonProperty("message")]
        public MusixResponseMessage Message { get; set; }

        public bool IsSuccessStatusCode()
        {
            return this.Message.Header.StatusCode == 200;
        }
    }

    public class MusixResponseMessage
    {
        [JsonProperty("header")]
        public Header Header { get; set; }
    }
}