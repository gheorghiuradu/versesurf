using ArtManager;
using LicensingService;
using MusicDbApi;
using MusicDbApi.Models;
using MusicStorageClient;
using MusixClient;
using SpotifyApiService;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskService.Extensions;
using TaskService.Jobs;

namespace TaskService
{
    public class ImportSpotifyPlaylistJob : Job<string>
    {
        private readonly SpotifyService spotifyService;
        private readonly LicenseService licenseService;
        private readonly MusicDbClient musicDbClient;
        private readonly MusixApiClient musixApiClient;
        private readonly GoogleStorage googleStorage;
        private readonly MusicBrainzApiClient musicBrainzApiClient;

        public ImportSpotifyPlaylistJob(
            SpotifyService spotifyService,
            LicenseService licenseService,
            MusixApiClient musixApiClient,
            GoogleStorage googleStorage,
            MusicBrainzApiClient musicBrainzApiClient,
            MusicDbClient musicDbClient)
        {
            this.spotifyService = spotifyService;
            this.licenseService = licenseService;
            this.musixApiClient = musixApiClient;
            this.googleStorage = googleStorage;
            this.musicBrainzApiClient = musicBrainzApiClient;
            this.musicDbClient = musicDbClient;
        }

        public override async Task RunAsync(IJobCommand internalCommand)
        {
            this.progress = internalCommand.Progress;
            var progressValue = 0;
            this.ReportStarted(internalCommand.JobId, $"Started {this.GetType().Name}");

            try
            {
                if (!(internalCommand is ImportSpotifyPlaylistCommand command))
                {
                    throw new InvalidOperationException("Wrong command type received");
                }

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                await this.spotifyService.InitializeAsync();

                var playlist = (await this.musicDbClient.GetPlaylistBySpotifyIdAsync(command.SpotifyId, internalCommand.Token)) ??
                                new Playlist
                                {
                                    SpotifyId = command.SpotifyId
                                };

                var spotifyPlaylist = this.spotifyService.GetPlaylist(command.SpotifyId);
                playlist.MapFrom(spotifyPlaylist);

                this.LastReport.DisplayName = $"Import playlist {playlist.Name}";
                this.ReportProgress(5, $"Starting work on playlist {playlist.SpotifyId} - {playlist.Name}");

                var tracks = this.spotifyService.GetTracks(command.SpotifyId).ToList();
                if (tracks.Count < command.MinimumNumberOfSongs)
                {
                    this.ReportFail($"Minimum required {command.MinimumNumberOfSongs} songs, " +
                        $"but Spotify playlist only had {tracks.Count()} tracks");
                    return;
                }

                progressValue = 6;
                this.ReportProgress(progressValue, $"Got {tracks.Count()} from Spotify");
                foreach (var playlistTrack in tracks)
                {
                    if (internalCommand.Token.IsCancellationRequested)
                    {
                        this.ReportCancel();
                        return;
                    }

                    var song = playlist.Songs.Find(s => string.Equals(s.SpotifyId, playlistTrack.Track.Id)) ??
                                playlistTrack.ToSong();

                    progressValue = tracks.IndexOf(playlistTrack) * 90 / tracks.Count;
                    this.ReportProgress(progressValue, $@"Starting work on song {song.Artist} - {song.Title}
                        (spotifyId: {song.SpotifyId} - dbId: {song.Id})");

                    var licenseCommand = new TryGetLicenseIdCommand
                    {
                        Artists = playlistTrack.Track.Artists.Select(a => a.Name).ToArray(),
                        CancellationToken = internalCommand.Token,
                        Title = playlistTrack.Track.Name
                    };
                    if (string.IsNullOrWhiteSpace(song.BmiLicenseId) && command.EnableBmi)
                    {
                        song.BmiLicenseId = await this.licenseService.TryGetBmiLicenseAsync(licenseCommand);
                        this.ReportProgress($"Bmi result: {song.BmiLicenseId}");

                        if (string.IsNullOrWhiteSpace(song.BmiLicenseId)
                            && string.IsNullOrWhiteSpace(song.ASCAPLicenseId)
                            && command.EnableAscap)
                        {
                            song.ASCAPLicenseId = await this.licenseService.TryGetAscapLicenseAsync(licenseCommand);
                            this.ReportProgress($"Ascap result: {song.ASCAPLicenseId}");

                            if (string.IsNullOrWhiteSpace(song.ASCAPLicenseId)
                                && string.IsNullOrWhiteSpace(song.SesacLicenseId)
                                && command.EnableSesac)
                            {
                                song.SesacLicenseId = await this.licenseService.TryGetSesacLicenseAsync(licenseCommand);
                                this.ReportProgress($"Sesac result: {song.SesacLicenseId}");

                                if (string.IsNullOrWhiteSpace(song.SesacLicenseId))
                                {
                                    this.ReportProgress($"Could not find any license for {song.Artist} - {song.Title} " +
                                        $"({song.SpotifyId})");
                                    continue;
                                }
                            }
                        }
                    }

                    if (string.IsNullOrWhiteSpace(song.Snippet))
                    {
                        song.Snippet = await this.musixApiClient.GetSnippetAsync(song.ISRC, song.Artist, song.Title);
                        if (string.IsNullOrWhiteSpace(song.Snippet))
                        {
                            this.ReportProgress($"Could not get snippet for {song.Artist} - {song.Title}");
                            continue;
                        }
                    }

                    song.PreviewUrl = await this.googleStorage.GetOrUploadSongPreviewAsync
                                (playlistTrack.Track.PreviewUrl, $"{song.SpotifyId}.mp3");

                    if (string.IsNullOrWhiteSpace(song.PlaylistId))
                    {
                        playlist.Songs.Add(song);
                    }
                    this.ReportProgress($@"Successfuly added or updated {song.Artist} - {song.Title}
                            (spotifyId: {song.SpotifyId} - dbId: {song.Id})");
                }

                // Remove old songs that are not found in the playlist anymore
                var songsSpotifyIds = tracks.Select(t => t.Track.Id);
                foreach (var song in playlist.Songs.Where(s => !songsSpotifyIds.Contains(s.SpotifyId)).ToArray())
                {
                    playlist.Songs.Remove(song);
                }

                this.ReportProgress(90, $"All songs processed ({playlist.Songs.Count} songs in playlist)");
                if (playlist.Songs.Count < command.MinimumNumberOfSongs)
                {
                    this.ReportFail($"Found licenses only for {playlist.Songs.Count} songs, {command.MinimumNumberOfSongs} required.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(playlist.PictureUrl))
                {
                    playlist.PictureUrl = await this.musicBrainzApiClient.GetImageForPlaylistAsync(playlist, internalCommand.Token);
                }

                if (string.IsNullOrWhiteSpace(playlist.Id))
                {
                    await this.musicDbClient.AddPlaylistAsync(playlist);
                }
                else
                {
                    await this.musicDbClient.UpdatePlaylistAsync(playlist);
                }

                this.ReportCompleted();
            }
            catch (TaskCanceledException)
            {
                this.ReportCancel();
                return;
            }
            catch (Exception ex)
            {
                this.ReportFail(ex.Message);
            }
        }
    }
}