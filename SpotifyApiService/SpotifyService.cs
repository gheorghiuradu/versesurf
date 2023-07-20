using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SpotifyApiService
{
    public class SpotifyService
    {
        private readonly SpotifyServiceOptions options;
        private SpotifyWebAPI spotifyWebApi;

        public SpotifyService(SpotifyServiceOptions options)
        {
            this.options = options;
        }

        public bool IsInitialized() => !(this.spotifyWebApi is null);

        public void Initialize()
        {
            var spotifyAuth = new CredentialsAuth(options.ClientId, options.ClientSecret);
            var spotifyToken = spotifyAuth.GetToken().Result;
            this.spotifyWebApi = new SpotifyWebAPI { TokenType = spotifyToken.TokenType, AccessToken = spotifyToken.AccessToken };
        }

        public async Task InitializeAsync()
        {
            var spotifyAuth = new CredentialsAuth(options.ClientId, options.ClientSecret);
            var spotifyToken = await spotifyAuth.GetToken();
            this.spotifyWebApi = new SpotifyWebAPI { TokenType = spotifyToken.TokenType, AccessToken = spotifyToken.AccessToken };
        }

        public bool PlaylistExists(string spotifyPlaylistId)
        {
            return this.spotifyWebApi.GetPlaylist(spotifyPlaylistId) is null;
        }

        public FullPlaylist GetPlaylist(string spotifyPlaylistId)
        {
            return this.spotifyWebApi.GetPlaylist(spotifyPlaylistId);
        }

        public IEnumerable<PlaylistTrack> GetTracks(string spotifyPlaylistId)
        {
            var spotifyTracksResult = spotifyWebApi.GetPlaylistTracks(spotifyPlaylistId, market: "us");
            var tracks = spotifyTracksResult
                .Items
                .Where(pt =>
                    !string.IsNullOrWhiteSpace(pt.Track?.PreviewUrl)
                    && Uri.IsWellFormedUriString(pt.Track?.PreviewUrl, UriKind.Absolute))
                .ToList();

            while (spotifyTracksResult.HasNextPage())
            {
                spotifyTracksResult = this.spotifyWebApi.GetNextPage(spotifyTracksResult);
                var newTracks = spotifyTracksResult
                            .Items
                            .Where(pt =>
                                !string.IsNullOrWhiteSpace(pt.Track?.PreviewUrl)
                                && Uri.IsWellFormedUriString(pt.Track?.PreviewUrl, UriKind.Absolute))
                            .ToList();
                tracks.AddRange(newTracks);
            }

            return tracks;
        }

        public IEnumerable<SimplePlaylist> GetAllCategoriesPlaylists(string countryCode)
        {
            var categories = this.spotifyWebApi.GetCategories(countryCode);
            var playlists = categories.Categories.Items.Select(c => this.spotifyWebApi.GetCategoryPlaylists(c.Id));

            return playlists.Where(p => p.Error is null)
                .SelectMany(p => p.Playlists.Items);
        }

        public IEnumerable<SimplePlaylist> GetFeaturedPlaylists(string countryCode)
        {
            var featured = this.spotifyWebApi.GetFeaturedPlaylists(country: countryCode);

            return featured.Playlists.Items;
        }

        public FullTrack GetTrack(string trackId)
        {
            return this.spotifyWebApi.GetTrack(trackId);
        }
    }
}