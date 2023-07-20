using MusicDbApi;
using MusicStorageClient;
using System;
using System.Linq;
using System.Threading.Tasks;
using TaskService.Commands;

namespace TaskService.Jobs
{
    public class CleanupUnusedStorageJob : Job
    {
        private readonly GoogleStorage googleStorage;
        private readonly MusicDbClient musicDbClient;

        public CleanupUnusedStorageJob(MusicDbClient musicDbClient, GoogleStorage googleStorage)
        {
            this.musicDbClient = musicDbClient;
            this.googleStorage = googleStorage;
        }

        public override async Task RunAsync(IJobCommand internalCommand)
        {
            this.progress = internalCommand.Progress;
            this.ReportStarted(internalCommand.JobId, $"Started {this.GetType().Name}");

            try
            {
                if (!(internalCommand is CleanupUnusedStorageCommand command))
                {
                    throw new InvalidOperationException("Wrong command type received");
                }

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                var allPlaylists = await this.musicDbClient.GetAllPlaylistsAsync(includeExplicit: true);
                this.ReportProgress(10, $"Got {allPlaylists.Count} playlists");
                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }
                var allPlaylistImages = await this.googleStorage.GetAllPlaylistImagesAsync();
                this.ReportProgress(20, $"Got {allPlaylistImages.Count} playlist images");
                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }
                var allSongPreviews = await this.googleStorage.GetAllSongPreviewsAsync();
                this.ReportProgress(30, $"Got {allPlaylists.Select(p => p.Songs).Count()} songs");
                this.ReportProgress($"Got {allSongPreviews.Count} song previews");
                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }
                var unusedImages = allPlaylistImages.Except(allPlaylists.Select(p => p.PictureUrl));
                var unusedSongPreviews = allSongPreviews.Except(allPlaylists.SelectMany(p => p.Songs.Select(s => s.PreviewUrl)));
                this.ReportProgress(40, $"Got {unusedImages.Count()} unused images");
                this.ReportProgress($"Got {unusedSongPreviews.Count()} unused song previews");
                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                var progress = 0;
                if (unusedImages.Any())
                {
                    foreach (var unusedImage in unusedImages)
                    {
                        if (internalCommand.Token.IsCancellationRequested)
                        {
                            this.ReportCancel();
                            return;
                        }
                        await this.googleStorage.DeleteObjectByUrlAsync(unusedImage);
                        progress++;
                        this.ReportProgress(progress);
                    }
                }
                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }
                if (unusedSongPreviews.Any())
                {
                    foreach (var preview in unusedSongPreviews)
                    {
                        if (internalCommand.Token.IsCancellationRequested)
                        {
                            this.ReportCancel();
                            return;
                        }
                        await this.googleStorage.DeleteObjectByUrlAsync(preview);
                        progress++;
                        this.ReportProgress(progress);
                    }
                }

                this.ReportCompleted($"Deleted {unusedImages.Count() + unusedSongPreviews.Count()} unused assets");
            }
            catch (TaskCanceledException)
            {
                this.ReportCancel();
                return;
            }
            catch (System.Exception ex)
            {
                this.ReportFail(ex.Message);
            }
        }
    }
}