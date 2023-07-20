using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RestSharp;
using SharedDomain.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApi.Serverless.Client
{
    public class MusicDbApiClient : MusicApiServerlessClient
    {
        public MusicDbApiClient(MusicApiServerlessOptions options) : base(options)
        {
        }

        public async ValueTask<List<IPlaylistViewModel>> GetEnabledPlaylistsAsync(
            string language = null,
            bool includeExplicit = false,
            PlaylistViewModelType type = PlaylistViewModelType.Simple)

        {
            this.logger.LogDebug("Starting request to get all enabled playlists");

            var request = new RestRequest("/playlist/GetEnabled")
                .AddParameter(nameof(includeExplicit), includeExplicit, ParameterType.QueryString)
                .AddParameter(nameof(language), language, ParameterType.QueryString)
                .AddParameter("type", type, ParameterType.QueryString);

            var response = await this.restClient.ExecuteAsync(request);
            this.LogIfError(response);

            return type switch
            {
                PlaylistViewModelType.Simple =>
                    JsonConvert.DeserializeObject<IEnumerable<PlaylistViewModel>>(response.Content).ToList<IPlaylistViewModel>(),
                PlaylistViewModelType.Full =>
                    JsonConvert.DeserializeObject<IEnumerable<FullPlaylistViewModel>>(response.Content).ToList<IPlaylistViewModel>(),
                _ => throw new System.NotImplementedException()
            };
        }

        public async ValueTask<IPlaylistViewModel> GetPlaylistByIdAsync(string playlistId,
            PlaylistViewModelType type = PlaylistViewModelType.Simple)
        {
            this.logger.LogDebug($"Starting request to get playlist by id: {playlistId}");

            if (string.IsNullOrWhiteSpace(playlistId))
            {
                this.logger.LogDebug("Received invalid id");
                return null;
            }

            var request = new RestRequest("/playlist/GetById/{id}");
            request.AddUrlSegment("id", playlistId);
            request.AddParameter("type", type, ParameterType.QueryString);

            var response = await this.restClient.ExecuteAsync(request);
            this.LogIfError(response);

            return type switch
            {
                PlaylistViewModelType.Simple =>
                    JsonConvert.DeserializeObject<PlaylistViewModel>(response.Content),
                PlaylistViewModelType.Full =>
                    JsonConvert.DeserializeObject<FullPlaylistViewModel>(response.Content),
                _ => throw new System.NotImplementedException()
            };
        }

        public async ValueTask<IPlaylistViewModel> GetRandomPlaylistAsync(bool includeExplicit = false,
            string language = null,
            bool onlyEnabled = true, PlaylistViewModelType type = PlaylistViewModelType.Simple)
        {
            this.logger.LogDebug("Starting request to get random playlist");

            var request = new RestRequest("/playlist/GetRandom")
                    .AddParameter(nameof(includeExplicit), includeExplicit)
                    .AddParameter(nameof(language), language)
                    .AddParameter(nameof(onlyEnabled), onlyEnabled)
                    .AddParameter(nameof(type), type);

            var response = await this.restClient.ExecuteAsync(request);
            this.LogIfError(response);

            return type switch
            {
                PlaylistViewModelType.Simple =>
                    JsonConvert.DeserializeObject<PlaylistViewModel>(response.Content),
                PlaylistViewModelType.Full =>
                    JsonConvert.DeserializeObject<FullPlaylistViewModel>(response.Content),
                _ => throw new System.NotImplementedException()
            };
        }
    }
}