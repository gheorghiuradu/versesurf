using MusicDbApi.Models;
using MusicStorageClient;
using PexelsNet;
using SpotifyAPI.Web;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtManager
{
    public class PexelsApiClient : ICoverArtService
    {
        private const string ApiKey = "PEXELSAPIKEY";
        private readonly PexelsClient pexels = new PexelsClient(ApiKey);
        private readonly GoogleStorage storage = new GoogleStorage(new GoogleStorageOptions());
        private SpotifyWebAPI spotifyWebApi;

        public async Task<string> GetImageForPlaylistAsync(Playlist playlist, CancellationToken token = default)
        {
            var searchResult = await this.pexels.SearchAsync(playlist.Name);
            var url = await this.HandleResults(searchResult);
            if (!string.IsNullOrEmpty(url)) return url;

            if (this.spotifyWebApi is null)
            {
                this.spotifyWebApi = await new SpotifyBuilder().BuildDefaultAsync();
            }

            var tasks = playlist.Songs.Select(async s => await this.spotifyWebApi.GetTrackAsync(s.SpotifyId));
            var tracks = (await Task.WhenAll(tasks)).OrderByDescending(t => t.Popularity);

            var artistTasks = tracks.Where(t => !(t.Artists is null))
                .SelectMany(t => t.Artists.Select(a => a.Id)).Select(async id => await this.spotifyWebApi.GetArtistAsync(id));
            var artists = (await Task.WhenAll(artistTasks)).OrderByDescending(a => a.Popularity);

            foreach (var artist in artists)
            {
                searchResult = await this.pexels.SearchAsync(artist.Name);
                url = await this.HandleResults(searchResult);
                if (!string.IsNullOrEmpty(url)) return url;
            }
            foreach (var genre in artists.SelectMany(a => a.Genres))
            {
                searchResult = await this.pexels.SearchAsync(genre);
                url = await this.HandleResults(searchResult);
                if (!string.IsNullOrEmpty(url)) return url;
            }

            return string.Empty;
        }

        private async Task<string> HandleResults(Page searchResult)
        {
            if (searchResult.Photos.Count > 0)
            {
                foreach (var result in searchResult.Photos)
                {
                    Uri.TryCreate(result.Src.Medium, UriKind.Absolute, out var uri);
                    var extension = Path.GetExtension(uri.LocalPath);
                    var fileKey = $"{result.Id}{extension}";
                    var exists = await this.storage.PlaylistImageExistsAsync(fileKey);
                    if (!exists)
                    {
                        return await this.storage.UploadPlaylistImageAsync(result.Src.Medium, fileKey);
                    }
                }
            }

            return string.Empty;
        }
    }
}