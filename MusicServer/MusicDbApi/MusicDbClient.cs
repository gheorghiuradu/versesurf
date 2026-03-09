using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MusicDbApi.Models;

namespace MusicDbApi;

public sealed class MusicDbClient(IConfiguration configuration, ILogger<MusicDbClient> logger)
{
    private readonly string _playlistsLocation = configuration["PlaylistsDir"] ?? "Playlists";
    private readonly Random _random = new();

    public async Task<string> AddPlaylistAsync(Playlist playlist, CancellationToken token = default)
    {
        var playlists = await LoadPlaylists();

        if (string.IsNullOrEmpty(playlist.Id)) playlist.Id = Guid.NewGuid().ToString();
        playlist.AddedAt = DateTime.UtcNow;
        playlist.UpdatedAt = DateTime.UtcNow;

        playlists.Add(playlist);
        throw new NotImplementedException();

        return playlist.Id;
    }

    public async Task<Song> GetSong(string id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return null;
        var playlists = await LoadPlaylists();
        if (token.IsCancellationRequested) return null;

        return playlists.SelectMany(p => p.Songs).FirstOrDefault(s => s.Id == id);
    }

    public async Task UpdatePlaylistAsync(Playlist playlist, CancellationToken token = default)
    {
        var playlists = await LoadPlaylists();
        playlist.UpdatedAt = DateTime.UtcNow;
        var existing = playlists.FirstOrDefault(p => p.Id == playlist.Id);
        // if (existing != null)
        // {
        //     existing.Name = playlist.Name;
        //     existing.Enabled = playlist.Enabled;
        //     existing.Featured = playlist.Featured;
        //     existing.PictureUrl = playlist.PictureUrl;
        //     existing.Songs = playlist.Songs;
        //     existing.Votes = playlist.Votes;
        //     existing.Language = playlist.Language;
        //     existing.Plays = playlist.Plays;
        //     existing.UpdatedAt = playlist.UpdatedAt;
        // }

        throw new NotImplementedException();
    }

    public async Task IncrementPlaylistMetadataAsync(IEnumerable<Playlist> playlists, CancellationToken token = default) => throw new NotImplementedException();

    // foreach (var playlist in playlists)
    // {
    //     var existing = await this.db.Playlists.FindAsync(new object[] { playlist.Id }, token);
    //     if (existing != null)
    //     {
    //         existing.Votes += playlist.Votes;
    //         existing.Plays += playlist.Plays;
    //     }
    // }
    // await this.db.SaveChangesAsync(token);
    public async Task<List<Playlist>> GetEnabledPlaylistsAsync(string language = null, bool includeExplicit = false, int numberOfSongs = 5, CancellationToken token = default)
    {
        var playlists = await LoadPlaylists();
        var result = playlists
            .Where(p => p.Enabled && p.Songs.Count >= numberOfSongs);

        if (!includeExplicit) result = result.Where(p => !p.Songs.Any(s => s.IsExplicit));
        if (!string.IsNullOrEmpty(language)) result = result.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));

        return result.OrderByDescending(p => p.Featured).ThenByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Plays).ToList();
    }

    public async Task<List<Playlist>> GetAllPlaylistsAsync(string language = null, bool includeExplicit = false, CancellationToken token = default)
    {
        var playlists = await LoadPlaylists();
        IEnumerable<Playlist> result = playlists;
        if (!includeExplicit) result = result.Where(p => !p.Songs.Any(s => s.IsExplicit));
        if (!string.IsNullOrEmpty(language)) result = result.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));

        return result.OrderByDescending(p => p.Featured).ThenByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Plays).ToList();
    }

    public async Task<Playlist> GetRandomPlaylistAsync(string language = null, bool includeExplicit = false, bool onlyEnabled = true)
    {
        var playlists = await LoadPlaylists();
        var query = playlists.AsQueryable();
        if (onlyEnabled) query = query.Where(p => p.Enabled);
        playlists = query.ToList();

        IEnumerable<Playlist> result = playlists;
        if (!includeExplicit) result = result.Where(p => !p.Songs.Any(s => s.IsExplicit));
        if (!string.IsNullOrEmpty(language)) result = result.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));

        var list = result.ToList();
        return list.ElementAt(_random.Next(0, list.Count));
    }

    public async Task<Playlist> GetPlaylistByIdAsync(string id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return null;
        var playlists = await LoadPlaylists();
        return playlists
            .FirstOrDefault(p => p.Id == id);
    }

    public async Task<List<Playlist>> GetPlaylistsByIdAsync(IEnumerable<string> ids, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return null;
        var playlists = await LoadPlaylists();
        return playlists
            .Where(p => ids.Contains(p.Id))
            .ToList();
    }

    public async Task<Playlist> GetPlaylistBySpotifyIdAsync(string id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return null;
        var playlists = await LoadPlaylists();
        return playlists
            .FirstOrDefault(p => p.SpotifyId == id);
    }

    public async Task<bool> PlaylistExistsByIdAsync(string id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return false;
        var playlists = await LoadPlaylists();
        return playlists.Any(p => p.Id == id);
    }

    public async Task<bool> PlaylistExistsBySpotifyIdAsync(string id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return false;
        var playlists = await LoadPlaylists();
        return playlists.Any(p => p.SpotifyId == id);
    }

    public async Task DeletePlaylistAsync(string id, CancellationToken token = default)
    {
        if (token.IsCancellationRequested) return;
        var playlists = await LoadPlaylists();
        var playlist = playlists.Find(p => p.Id == id);
        if (playlist != null)
        {
            playlists.Remove(playlist);
            throw new NotImplementedException();
        }
    }

    private static void EnsureSongsHaveIds(IEnumerable<Song> songs)
    {
        if (songs == null) return;
        foreach (var song in songs)
            if (string.IsNullOrEmpty(song.Id))
                song.Id = Guid.NewGuid().ToString();
    }

    private async Task<List<Playlist>> LoadPlaylists()
    {
        List<Playlist> playlists = [];
        foreach (var file in Directory.EnumerateFiles(_playlistsLocation, "*.json"))
            try
            {
                var playlist = JsonSerializer.Deserialize<Playlist>(await File.ReadAllTextAsync(file));
                if (playlist == null) continue;

                if (string.IsNullOrEmpty(playlist.Id)) playlist.Id = file.Split('_').FirstOrDefault() ?? Guid.NewGuid().ToString();
                EnsureSongsHaveIds(playlist.Songs);
                playlists.Add(playlist);
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Could not load playlist file {File}", file);
            }

        return playlists;
    }
}