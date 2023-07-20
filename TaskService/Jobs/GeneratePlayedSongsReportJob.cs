using ClosedXML.Excel;
using Microsoft.Extensions.Hosting;
using MusicDbApi;
using MusicEventDbApi;
using PlayFab.ServerModels;
using PlayFabService;
using SharedDomain.InfraEvents;
using SharedDomain.Messages.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TaskService.Commands;
using TaskService.Extensions;

using Path = System.IO.Path;

namespace TaskService.Jobs
{
    public class GeneratePlayedSongsReportJob : Job<Uri>
    {
        private const string FolderName = "reports";

        private readonly MusicEventDbClient musicEventDbClient;
        private readonly EconomyService economyService;
        private readonly MusicDbClient musicDbClient;
        private readonly IHostEnvironment hostEnvironment;

        public GeneratePlayedSongsReportJob(
            MusicEventDbClient musicEventDbClient,
            EconomyService economyService,
            MusicDbClient musicDbClient,
            IHostEnvironment hostEnvironment)
        {
            this.musicEventDbClient = musicEventDbClient;
            this.economyService = economyService;
            this.musicDbClient = musicDbClient;
            this.hostEnvironment = hostEnvironment;
        }

        public override async Task RunAsync(IJobCommand internalCommand)
        {
            this.progress = internalCommand.Progress;

            this.ReportStarted(internalCommand.JobId, $"Started {this.GetType().Name}");

            try
            {
                if (!(internalCommand is GeneratePlayedSongsReportCommand command))
                {
                    throw new InvalidOperationException("Wrong command type received");
                }

                if (internalCommand.Token.IsCancellationRequested)
                {
                    this.ReportCancel();
                    return;
                }

                var fileName = $"report_{command.StartDate.ToShortDateString()}_{command.EndDate.ToShortDateString()}.xlsx";
                var folderPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot", FolderName);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                var filePath = Path.Combine(folderPath, fileName);
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.AddWorksheet();
                    worksheet.Cell(1, 1).SetValue("Event");
                    worksheet.Cell(1, 2).SetValue("TimeStampUtc");
                    worksheet.Cell(1, 3).SetValue("SongId");
                    worksheet.Cell(1, 4).SetValue("SongArtist");
                    worksheet.Cell(1, 5).SetValue("SongTitle");
                    worksheet.Cell(1, 6).SetValue("SongBMIId");
                    worksheet.Cell(1, 7).SetValue("SongAscapId");
                    worksheet.Cell(1, 8).SetValue("SongSesacId");
                    worksheet.Cell(1, 9).SetValue("PlaylistId");
                    worksheet.Cell(1, 10).SetValue("PlaylistName");
                    worksheet.Cell(1, 11).SetValue("RoomCode");
                    worksheet.Cell(1, 12).SetValue("PlayFabId");
                    worksheet.Cell(1, 13).SetValue("CountryCode");
                    workbook.SaveAs(filePath);
                }

                var playlists = await this.musicDbClient.GetAllPlaylistsAsync(includeExplicit: true, token: internalCommand.Token);
                var timespan = command.EndDate - command.StartDate;
                var dates = new List<DateTime>();
                for (var dt = command.StartDate; dt <= command.EndDate; dt = dt.AddDays(1))
                {
                    dates.Add(dt);
                }

                var nextRow = 2;
                foreach (var date in dates)
                {
                    if (internalCommand.Token.IsCancellationRequested)
                    {
                        this.ReportCancel();
                        return;
                    }
                    var progressValue = dates.IndexOf(date) / timespan.TotalDays * 95;
                    this.ReportProgress((int)progressValue);
                    var events = await this.musicEventDbClient.GetEventsByDateAsync(
                        date,
                        date.AddHours(24),
                        new List<string> { nameof(EventType.GameStarted), nameof(EventType.CreatedRoom), nameof(EventType.PlayedSong) },
                        internalCommand.Token);

                    var domaineEvents = events.Select(e => e.ToDMEvent());
                    var bookRoomMessages = domaineEvents
                        .Where(e => e.PayloadJson.Contains("PlayFabId", StringComparison.OrdinalIgnoreCase)
                                && e.PayloadJson.Contains("RoomCode", StringComparison.OrdinalIgnoreCase))
                        .Select(e => e.GetPayloadAsFlatDictionary());
                    if (!bookRoomMessages.Any())
                    {
                        continue;
                    }
                    var playfabIds = bookRoomMessages
                        .Select(e =>
                        {
                            var key = e.Keys.FirstOrDefault(k => k.Equals("PlayFabId", StringComparison.OrdinalIgnoreCase));
                            return e[key];
                        });
                    var playedSongEvents = domaineEvents.Where(e => e.EventType.Equals(nameof(EventType.PlayedSong))).ToList();
                    if (playedSongEvents.Count == 0)
                    {
                        continue;
                    }

                    var playerProfiles = new List<PlayerProfileModel>();
                    foreach (var player in playfabIds)
                    {
                        var profile = await this.economyService.GetPlayerProfileAsync(player);
                        playerProfiles.Add(profile);
                    }

                    for (int playedCount = 0; playedCount < playedSongEvents.Count; playedCount++)
                    {
                        if (internalCommand.Token.IsCancellationRequested)
                        {
                            this.ReportCancel();
                            return;
                        }

                        var @event = playedSongEvents[playedCount];
                        var askMessage = @event.GetPayloadAs<AskMessage>();
                        if (string.IsNullOrWhiteSpace(askMessage.SongId))
                        {
                            continue;
                        }

                        var bookRoomMessage = bookRoomMessages.FirstOrDefault(g => g["RoomCode"].Equals(askMessage.RoomCode));
                        if (bookRoomMessage is null)
                        {
                            continue;
                        }
                        var playfabKey = bookRoomMessage.Keys.FirstOrDefault(k => k.Equals("PlayFabId", StringComparison.OrdinalIgnoreCase));
                        var playFabId = bookRoomMessage[playfabKey];
                        var playlist = playlists.Find(p => !(p.Songs.Find(s => s.Id.Equals(askMessage.SongId)) is null));
                        var song = playlist?.Songs?.Find(s => s.Id.Equals(askMessage.SongId));
                        var playerProfile = playerProfiles.Find(pp => pp.PlayerId.Equals(playFabId));

                        using (var workbook = new XLWorkbook(filePath))
                        {
                            var worksheet = workbook.Worksheets.Worksheet(1);
                            worksheet.Cell(nextRow, 1).SetValue(@event.EventType);
                            worksheet.Cell(nextRow, 2).SetValue(@event.TimeStamp);
                            worksheet.Cell(nextRow, 3).SetValue(askMessage.SongId);
                            worksheet.Cell(nextRow, 4).SetValue(song?.Artist);
                            worksheet.Cell(nextRow, 5).SetValue(song?.Title);
                            worksheet.Cell(nextRow, 6).SetValue(song?.BmiLicenseId);
                            worksheet.Cell(nextRow, 7).SetValue(song?.ASCAPLicenseId);
                            worksheet.Cell(nextRow, 8).SetValue(song?.SesacLicenseId);
                            worksheet.Cell(nextRow, 9).SetValue(playlist?.Id);
                            worksheet.Cell(nextRow, 10).SetValue(playlist?.Name);
                            worksheet.Cell(nextRow, 11).SetValue(askMessage.RoomCode);
                            worksheet.Cell(nextRow, 12).SetValue(playFabId);
                            worksheet.Cell(nextRow, 13).SetValue(playerProfile?.Locations?.FirstOrDefault()?.CountryCode);
                            workbook.Save();
                        }
                        nextRow++;
                    }
                    GC.Collect();
                }
                this.ReportCompleted(new Uri($"/{FolderName}/{fileName}", UriKind.Relative), "Report successfully generated.");
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