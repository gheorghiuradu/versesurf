using Ionic.Zip;
using Microsoft.Extensions.Hosting;
using MusicDbApi;
using MusicDbApi.Models;
using MusicStorageClient;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskService.Commands;

namespace TaskService.Jobs
{
    public class ImportPlaylistsJob : Job
    {
        private const string FolderName = "imports";

        private readonly MusicDbClient musicDbClient;
        private readonly GoogleStorage googleStorage;
        private readonly IHostEnvironment hostEnvironment;

        public ImportPlaylistsJob(MusicDbClient musicDbClient, GoogleStorage googleStorage, IHostEnvironment hostEnvironment)
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
                if (!(internalCommand is ImportPlaylistsCommand command))
                {
                    throw new InvalidOperationException("Wrong command type received");
                }

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                var directoryPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", FolderName);
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var importDirectory = Path.Combine(directoryPath, internalCommand.JobId);
                if (!Directory.Exists(importDirectory))
                {
                    Directory.CreateDirectory(importDirectory);
                }

                var zipPath = Path.Combine(importDirectory, $"{internalCommand.JobId}.zip");
                using (var file = File.Create(zipPath))
                {
                    command.ImportStream.Seek(0, SeekOrigin.Begin);
                    await command.ImportStream.CopyToAsync(file);
                    await command.ImportStream.DisposeAsync();
                }
                using (var zip = ZipFile.Read(zipPath))
                {
                    zip.ExtractAll(importDirectory, ExtractExistingFileAction.OverwriteSilently);
                }

                var allJsons = Directory.EnumerateFiles(importDirectory, "*.json").ToList();
                foreach (var playlistPath in allJsons)
                {
                    var playlistJson = await File.ReadAllTextAsync(playlistPath, internalCommand.Token);
                    var playlist = JsonConvert.DeserializeObject<Playlist>(playlistJson);
                    this.ReportProgress($"Importing playlist {playlist.Name}");

                    foreach (var song in playlist.Songs)
                    {
                        if (!string.IsNullOrEmpty(song.FullAudioUrl))
                        {
                            song.FullAudioUrl = await this.googleStorage.UploadSongAudioAsync(Path.Combine(importDirectory, song.FullAudioUrl));
                        }
                        if (!string.IsNullOrEmpty(song.PreviewUrl))
                        {
                            song.PreviewUrl = await this.googleStorage.UploadSongAudioAsync(Path.Combine(importDirectory, song.PreviewUrl));
                        }
                    }

                    if (!string.IsNullOrEmpty(playlist.PictureUrl))
                    {
                        playlist.PictureUrl = await this.googleStorage.UploadPlaylistImageAsync(Path.Combine(importDirectory, playlist.PictureUrl));
                    }

                    var existingPlaylist = await this.musicDbClient.GetPlaylistByIdAsync(playlist.Id, internalCommand.Token);
                    if (!(existingPlaylist is null))
                    {
                        await this.musicDbClient.UpdatePlaylistAsync(playlist, internalCommand.Token);
                    }
                    else
                    {
                        await this.musicDbClient.AddPlaylistAsync(playlist, internalCommand.Token);
                    }

                    this.ReportProgress(allJsons.IndexOf(playlistJson) / allJsons.Count * 99, $"Imported playlist {playlist.Name}");
                }

                Directory.Delete(importDirectory, true);

                this.ReportCompleted($"Imported {allJsons.Count} playlists");
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
    }
}