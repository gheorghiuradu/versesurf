using Ionic.Zip;
using Microsoft.Extensions.Hosting;
using MusicDbApi;
using MusicStorageClient;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskService.Commands;

namespace TaskService.Jobs
{
    public class ExportPlaylistsJob : Job<Uri>
    {
        private const string FolderName = "exports";

        private readonly MusicDbClient musicDbClient;
        private readonly GoogleStorage googleStorage;
        private readonly IHostEnvironment hostEnvironment;

        public ExportPlaylistsJob(MusicDbClient musicDbClient, GoogleStorage googleStorage, IHostEnvironment hostEnvironment)
        {
            this.musicDbClient = musicDbClient;
            this.googleStorage = googleStorage;
            this.hostEnvironment = hostEnvironment;
        }

        public override async Task RunAsync(IJobCommand internalCommand)
        {
            this.progress = internalCommand.Progress;

            this.ReportStarted(internalCommand.JobId, $"Started {this.GetType().Name}");

            try
            {
                if (!(internalCommand is ExportPlaylistsCommand command))
                {
                    throw new InvalidOperationException("Wrong command type received");
                }

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                var playlists = await this.musicDbClient.GetPlaylistsByIdAsync(command.PlaylistIds);
                this.LastReport.DisplayName = $"Export playlist(s) {string.Join(' ', playlists.Select(p => p.Name))}";

                var directoryPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", FolderName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
                var exportFolderPath = Path.Combine(directoryPath, internalCommand.JobId);
                if (!Directory.Exists(exportFolderPath))
                {
                    Directory.CreateDirectory(exportFolderPath);
                }

                foreach (var playlist in playlists)
                {
                    if (internalCommand.Token.IsCancellationRequested)
                    {
                        this.ReportCancel();
                        return;
                    }

                    foreach (var song in playlist.Songs)
                    {
                        if (!string.IsNullOrEmpty(song.FullAudioUrl))
                        {
                            var fullSongFileName = this.GetCleanFileName(song.FullAudioUrl);
                            await this.googleStorage.DownloadFileByUrlAsync(song.FullAudioUrl, Path.Combine(exportFolderPath, fullSongFileName));
                            song.FullAudioUrl = fullSongFileName;
                        }
                        if (!string.IsNullOrEmpty(song.PreviewUrl))
                        {
                            var songPreviewFileName = this.GetCleanFileName(song.PreviewUrl);
                            await this.googleStorage.DownloadFileByUrlAsync(song.PreviewUrl, Path.Combine(exportFolderPath, songPreviewFileName));
                            song.PreviewUrl = songPreviewFileName;
                        }
                    }

                    if (!string.IsNullOrEmpty(playlist.PictureUrl))
                    {
                        var pictureNewFileName = this.GetCleanFileName(playlist.PictureUrl);
                        await this.googleStorage.DownloadFileByUrlAsync(playlist.PictureUrl, Path.Combine(exportFolderPath, pictureNewFileName));
                        playlist.PictureUrl = pictureNewFileName;
                    }

                    var playlistJsonPath = Path.Combine(exportFolderPath, $"{playlist.Id}.json");
                    await File.WriteAllTextAsync(playlistJsonPath, JsonConvert.SerializeObject(playlist), internalCommand.Token);

                    this.ReportProgress(playlists.IndexOf(playlist) / playlists.Count * 90);
                }

                var exportFileName = $"{internalCommand.JobId}.zip";
                var exportFilePath = Path.Combine(directoryPath, exportFileName);
                using (var zip = new ZipFile(exportFilePath))
                {
                    zip.AddDirectory(exportFolderPath);
                    zip.Save();
                }
                Directory.Delete(exportFolderPath, true);

                this.ReportProgress("Setting up task to delete the generated export after 1 hour");
                Task.Run(async () => { await Task.Delay(TimeSpan.FromHours(1)).ConfigureAwait(false); File.Delete(exportFilePath); }).ConfigureAwait(false);

                this.ReportCompleted(new Uri($"/{FolderName}/{exportFileName}", UriKind.Relative), "Export successfully generated.");
            }
            catch (TaskCanceledException)
            {
                this.ReportCancel();
            }
            catch (Exception ex)
            {
                this.ReportFail(ex.Message);
            }
        }

        private string GetCleanFileName(string path)
        {
            var fileName = Path.GetFileName(path);
            if (fileName.Contains('?'))
            {
                fileName = fileName.Substring(0, fileName.IndexOf('?'));
            }
            if (fileName.Contains("%2F"))
            {
                fileName = fileName[(fileName.LastIndexOf("%2F") + 3)..];
            }

            return fileName;
        }
    }
}