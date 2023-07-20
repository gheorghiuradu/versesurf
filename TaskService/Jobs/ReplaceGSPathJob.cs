using MusicDbApi;
using System;
using System.Threading.Tasks;
using TaskService.Commands;

namespace TaskService.Jobs
{
    public class ReplaceGSPathJob : Job
    {
        private readonly MusicDbClient musicDbClient;

        public ReplaceGSPathJob(MusicDbClient musicDbClient)
        {
            this.musicDbClient = musicDbClient;
        }

        public override async Task RunAsync(IJobCommand internalCommand)
        {
            this.progress = internalCommand.Progress;
            this.ReportStarted(internalCommand.JobId, $"Started {this.GetType().Name}");

            try
            {
                if (!(internalCommand is ReplaceGSPathCommand command))
                {
                    throw new InvalidOperationException("Wrong command type received");
                }

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                if (string.IsNullOrWhiteSpace(command.PlaylistImageDestination)
                    && string.IsNullOrWhiteSpace(command.SongPreviewDestination))
                {
                    this.ReportFail("Both destinations cannot be empty");
                    return;
                }

                if (string.IsNullOrWhiteSpace(command.PlaylistImageSource) &&
                    string.IsNullOrWhiteSpace(command.SongPreviewSource))
                {
                    this.ReportFail("Both sources cannot be empty");
                    return;
                }

                var playlists = await this.musicDbClient.GetAllPlaylistsAsync(includeExplicit: true, token: internalCommand.Token);
                this.ReportProgress(10, $"Got {playlists.Count} playlists");

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }
                var progress = 10;
                foreach (var playlist in playlists)
                {
                    var updated = false;

                    if (internalCommand.Token.IsCancellationRequested)
                    {
                        this.ReportCancel();
                        return;
                    }

                    if (!string.IsNullOrWhiteSpace(command.PlaylistImageDestination)
                        && !string.IsNullOrWhiteSpace(command.PlaylistImageSource)
                        && playlist.PictureUrl.Contains(command.PlaylistImageSource, StringComparison.OrdinalIgnoreCase))
                    {
                        playlist.PictureUrl = playlist.PictureUrl.Replace(
                            command.PlaylistImageSource,
                            command.PlaylistImageDestination,
                            StringComparison.OrdinalIgnoreCase);
                        updated = true;
                    }
                    if (command.RemoveQueryParametersPlaylistImage &&
                        playlist.PictureUrl.Contains("?"))
                    {
                        playlist.PictureUrl = playlist.PictureUrl.Split("?")[0];
                        updated = true;
                    }

                    foreach (var song in playlist.Songs)
                    {
                        if (!string.IsNullOrWhiteSpace(command.SongPreviewDestination)
                            && !string.IsNullOrWhiteSpace(command.SongPreviewSource)
                            && song.PreviewUrl.Contains(command.SongPreviewSource, StringComparison.OrdinalIgnoreCase))
                        {
                            song.PreviewUrl = song.PreviewUrl.Replace(
                                command.SongPreviewSource,
                                command.SongPreviewDestination,
                                StringComparison.OrdinalIgnoreCase);
                            updated = true;
                        }
                        if (command.RemoveQueryParametersSongPreview
                            && song.PreviewUrl.Contains("?"))
                        {
                            song.PreviewUrl = song.PreviewUrl.Split("?")[0];
                            updated = true;
                        }
                    }

                    if (internalCommand.Token.IsCancellationRequested)
                    {
                        this.ReportCancel();
                        return;
                    }

                    if (updated)
                    {
                        await this.musicDbClient.UpdatePlaylistAsync(playlist, internalCommand.Token);
                    }
                    progress++;
                    this.ReportProgress(progress);
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