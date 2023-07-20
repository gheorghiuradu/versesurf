using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace MusixClient
{
    public class MusixApiClient
    {
        public const string ConfigKeyForApiKey = "MusixApiKey";

        private const string BaseUrl = "https://api.musixmatch.com/ws/1.1";
        private readonly string apiKey;
        private readonly HttpClient client = new HttpClient();

        public MusixApiClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public MusixApiClient(IConfiguration configuration, string configKey = ConfigKeyForApiKey)
        {
            this.apiKey = configuration[configKey];
        }

        public async Task<string> GetSnippetAsync(string isrc, string artist, string title)
        {
            long? id = null;
            var query = $"{BaseUrl}/track.get?track_isrc={isrc}&apikey={this.apiKey}";
            var isrcResponseJson = await this.client.GetStringAsync(query);
            var isrcResponse = JsonConvert.DeserializeObject<MusixResponse>(isrcResponseJson);

            if (!isrcResponse.IsSuccessStatusCode())
            {
                query = $"{BaseUrl}/track.search?apikey={this.apiKey}&q_track={title}&q_artist={artist}&s_track_rating=desc";
                var searchResponseJson = await this.client.GetStringAsync(query);
                var searchResponse = JsonConvert.DeserializeObject<MusixResponse>(searchResponseJson);

                if (!searchResponse.IsSuccessStatusCode())
                {
                    return string.Empty;
                }
                else
                {
                    var searchResults = JsonConvert.DeserializeObject<SearchResponse>(searchResponseJson);
                    id = searchResults.Message.Body.TrackList.ToList()
                        .Find(t => string.Equals(t.TrackName, title, StringComparison.OrdinalIgnoreCase))
                        ?.TrackId;

                    if (id is null) return string.Empty;
                }
            }
            else
            {
                id = JsonConvert.DeserializeObject<TrackResponse>(isrcResponseJson)
                    .Message.Body?.Track?.TrackId;
            }

            query = $"{BaseUrl}/track.snippet.get?apikey={this.apiKey}&track_id={id}";
            var snippetResponseJson = await this.client.GetStringAsync(query);
            var snippetResponse = JsonConvert.DeserializeObject<MusixResponse>(snippetResponseJson);

            if (!snippetResponse.IsSuccessStatusCode())
            {
                return string.Empty;
            }

            return JsonConvert.DeserializeObject<SnippetResponse>(snippetResponseJson).Message.Body?.Snippet?.SnippetBody;
        }
    }
}