using ArtManager.Models;
using MetaBrainz.MusicBrainz;
using MetaBrainz.MusicBrainz.Interfaces.Entities;
using MusicDbApi.Models;
using MusicStorageClient;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ArtManager
{
    public class MusicBrainzApiClient : ICoverArtService
    {
        private SpotifyWebAPI spotifyWebAPI;
        private readonly GoogleStorage googleStorage;

        private readonly HttpClient httpClient = new HttpClient
        {
            BaseAddress = new Uri("http://coverartarchive.org")
        };

        public MusicBrainzApiClient(GoogleStorage googleStorage)
        {
            this.httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            this.httpClient.DefaultRequestHeaders.Add("User-Agent", "verse.surf  Version/1 mymailatmaildotcom");
            this.googleStorage = googleStorage;
        }

        public async Task<string> GetImageForPlaylistAsync(Playlist playlist, CancellationToken token = default)
        {
            if (token.IsCancellationRequested)
            {
                throw new TaskCanceledException();
            }

            var fileKey = string.IsNullOrWhiteSpace(playlist.SpotifyId) ? $"{playlist.Id}.jpg" : $"{playlist.SpotifyId}.jpg";
            if (await this.googleStorage.PlaylistImageExistsAsync(fileKey))
            {
                var existingUrl = await this.googleStorage.GetPlaylistImageUrlAsync(fileKey);
                using var web = new WebClient();
                web.DownloadFile(existingUrl, fileKey);
                ImageProcessingService.ResizeImage(fileKey);

                string newUrl;
                using (var stream = File.OpenRead(fileKey))
                {
                    newUrl = await this.googleStorage.UploadPlaylistImageAsync(stream, fileKey);
                }
                File.Delete(fileKey);

                return newUrl;
            }

            if (this.spotifyWebAPI is null)
            {
                this.spotifyWebAPI = await new SpotifyBuilder().BuildDefaultAsync();
            }

            var spotifyTracks = new List<FullTrack>();
            if (string.IsNullOrWhiteSpace(playlist.SpotifyId))
            {
                foreach (var song in playlist.Songs)
                {
                    if (!string.IsNullOrWhiteSpace(song.SpotifyId))
                    {
                        var track = await this.spotifyWebAPI.GetTrackAsync(song.SpotifyId);
                        spotifyTracks.Add(track);
                    }
                }
            }
            else
            {
                spotifyTracks = (await this.spotifyWebAPI.GetPlaylistTracksAsync(playlist.SpotifyId)).Items.ConvertAll(pt => pt.Track);
            }
            var queue = new Queue<FullTrack>(spotifyTracks.OrderByDescending(pt => pt.Popularity));

            var query = new Query("verse.surf", "1.0.0", "mailto:gheorghiuradu@outlook.com");

            while (queue.Count > 0)
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }
                var imageUrl = string.Empty;

                var nextTrack = queue.Dequeue();
                var releases = await query
                    .FindReleasesAsync($"release:{nextTrack.Name} AND artistname:{nextTrack.Artists[0].Name}");
                foreach (var release in releases.Results)
                {
                    imageUrl = await this.TryGetCoverArtImageAsync(release.Item);
                    if (!string.IsNullOrWhiteSpace(imageUrl)) break;
                }

                if (imageUrl is null)
                {
                    var releaseGroups = await query
                        .FindReleaseGroupsAsync
                        ($"releasegroup:{nextTrack.Album.Name} AND artistname:{nextTrack.Artists[0].Name}");
                    foreach (var releaseGroup in releaseGroups.Results)
                    {
                        imageUrl = await this.TryGetCoverArtImageAsync(releaseGroup.Item);
                        if (!string.IsNullOrWhiteSpace(imageUrl)) break;
                    }
                }

                if (string.IsNullOrWhiteSpace(imageUrl))
                {
                    continue;
                }

                using (var web = new WebClient())
                {
                    web.DownloadFile(imageUrl, fileKey);
                }

                ImageProcessingService.ResizeImage(fileKey);

                string publicUrl;
                using (var stream = File.OpenRead(fileKey))
                {
                    publicUrl = await this.googleStorage.UploadPlaylistImageAsync(stream, fileKey);
                }
                File.Delete(fileKey);

                return publicUrl;
            }

            return string.Empty;
        }

        private async Task<string> TryGetCoverArtImageAsync(IEntity entity)
        {
            var imageUrl = string.Empty;
            var requestUrl = string.Empty;

            if (entity is IRelease)
            {
                requestUrl = "/release";
            }
            if (entity is IReleaseGroup)
            {
                requestUrl = "/release-group";
            }
            try
            {
                var json = await this.httpClient.GetStringAsync($"{requestUrl}/{entity.Id}");
                if (!string.IsNullOrWhiteSpace(json))
                {
                    var front = JsonConvert.DeserializeObject<CoverArtArchiveResponse>(json)
                        .Images.Find(i => i.Front);

                    if (!(front is null))
                    {
                        imageUrl = front.ImageImage?.ToString();
                    }
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(2));

            return imageUrl;
        }
    }
}