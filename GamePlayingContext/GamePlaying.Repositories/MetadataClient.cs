using GamePlaying.Domain.PlaylistMetadataAggregate;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using MusicApi.Serverless.Client;
using RestSharp;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamePlaying.Repositories
{
    public class MetadataClient : MusicApiServerlessClient
    {
        public MetadataClient(MusicApiServerlessOptions options) : base(options)
        {
        }

        public async Task<bool> UpdatePlaylistMetadata(IEnumerable<PlaylistMetadata> playlistsMetadata)
        {
            this.logger.LogDebug("Starting request to update given playlists metadata");

            var request = new RestRequest("/playlist/metadata", Method.POST);
            request.AddJsonBody(playlistsMetadata);

            var response = await this.restClient.ExecuteAsync(request);
            this.LogIfError(response, request);

            return response.StatusCode.Equals(StatusCodes.Status204NoContent);
        }
    }
}