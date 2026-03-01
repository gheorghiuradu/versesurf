using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Logging;
using MusicDbApi.Models;
using MusicServer.Models;

namespace MusicServer.Services;

public sealed partial class AssetService(HttpClient httpClient, IWebHostEnvironment env, IServer server, ILogger<AssetService> logger)
{
    private const string CacheDir = "cache";
    private const string SpotifyEmbedUrlTemplate = "https://open.spotify.com/embed/track/{0}";
    private const string CoverSuffix = "_cover.jpg";
    private const string PreviewSuffix = "_preview.mp3";
    private const string NextDataSuffix = "_nextdata.json";
    private static readonly Regex NextDataScriptRegex = NextDataRegex();
    private readonly string _cachePath = Path.Combine(env.ContentRootPath, CacheDir);

    public async Task<string> GetCoverImage(Song song, bool retry = true)
    {
        var cacheKey = $"{song.Id}{CoverSuffix}";
        try
        {
            string asset;
            if (TryGetFromCache(cacheKey, out var coverImage))
                asset = coverImage;
            else
                asset = await GetSpotifyAsset(song,
                    nextData => nextData?.Props?.PageProps?.State?.Data?.Entity?.VisualIdentity?.Image?.FirstOrDefault()?.Url?.ToString(),
                    CoverSuffix, retry);


            return ToServerPath(asset);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting cover image");
            TryRemoveFromCache(cacheKey);

            if (retry) return await GetCoverImage(song, false);

            return null;
        }
    }

    public async Task<string> GetSongPreview(Song song, bool retry = true)
    {
        var cacheKey = $"{song.Id}{PreviewSuffix}";
        try
        {
            string asset;
            if (TryGetFromCache(cacheKey, out var previewPath))
                asset = previewPath;
            else
                asset = await GetSpotifyAsset(song,
                    nextData => nextData?.Props?.PageProps?.State?.Data?.Entity?.AudioPreview?.Url?.ToString(),
                    PreviewSuffix, retry);

            return ToServerPath(asset);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting song preview");
            TryRemoveFromCache(cacheKey);

            if (retry) return await GetSongPreview(song, false);

            return null;
        }
    }

    private async Task<string> GetSpotifyAsset(Song song, Func<SpotifyNextDataTrack, string> assetSelector, string suffix, bool retry = true)
    {
        var cacheKey = $"{song.Id}{suffix}";
        try
        {
            var nextData = await GetSpotifyNextData(song.SpotifyId);
            var assetUrl = assetSelector(nextData);
            if (assetUrl is null) return null;

            var assetBytes = await httpClient.GetByteArrayAsync(assetUrl);
            return await TryAddToCache(cacheKey, assetBytes);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting Spotify asset");
            TryRemoveFromCache(cacheKey);

            if (retry) return await GetSpotifyAsset(song, assetSelector, suffix, false);

            return null;
        }
    }

    private async Task<SpotifyNextDataTrack> GetSpotifyNextData(string spotifyTrackId)
    {
        var cacheKey = $"{spotifyTrackId}{NextDataSuffix}";
        if (TryGetFromCache(cacheKey, out var filePath))
        {
            var cachedJson = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<SpotifyNextDataTrack>(cachedJson);
        }

        var spotifyEmbedUrl = string.Format(SpotifyEmbedUrlTemplate, spotifyTrackId);
        var response = await httpClient.GetAsync(spotifyEmbedUrl);
        if (!response.IsSuccessStatusCode) return null;

        var html = await response.Content.ReadAsStringAsync();
        var match = NextDataScriptRegex.Match(html);
        if (!match.Success) return null;

        var json = match.Groups["json"].Value;
        await TryAddToCache(cacheKey, json);

        return JsonSerializer.Deserialize<SpotifyNextDataTrack>(json);
    }

    private async Task<string> TryAddToCache(string key, byte[] data)
    {
        try
        {
            Directory.CreateDirectory(_cachePath);
            var filePath = Path.Combine(_cachePath, key);
            await File.WriteAllBytesAsync(filePath, data);

            return filePath;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding file");
            return null;
        }
    }

    private async Task TryAddToCache(string key, string value)
    {
        try
        {
            Directory.CreateDirectory(_cachePath);
            var filePath = Path.Combine(_cachePath, key);
            await File.WriteAllTextAsync(filePath, value);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding file");
        }
    }

    private bool TryGetFromCache(string key, out string filePath)
    {
        try
        {
            filePath = Path.Combine(_cachePath, key);
            return File.Exists(filePath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting file");
            filePath = null;
            return false;
        }
    }

    private void TryRemoveFromCache(string key)
    {
        try
        {
            var filePath = Path.Combine(_cachePath, key);
            if (!File.Exists(filePath)) return;

            File.Delete(filePath);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting file");
        }
    }

    private string ToServerPath(string filePath)
    {
        var serverAddress = server.Features.Get<IServerAddressesFeature>()?.Addresses.FirstOrDefault();
        if (serverAddress is null) throw new InvalidOperationException("Server address not found");

        var relativePath = Path.GetRelativePath(env.ContentRootPath, filePath).Replace("\\", "/");
        return $"{serverAddress}/{relativePath}";
    }

    [GeneratedRegex("""<script id="__NEXT_DATA__" type="application/json">(?<json>.+?)</script>""", RegexOptions.Compiled)]
    private static partial Regex NextDataRegex();
}