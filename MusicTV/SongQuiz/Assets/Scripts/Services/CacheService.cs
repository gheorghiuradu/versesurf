using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Extensions;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Services
{
    public class CacheService
    {
        private const int MaxNumberOfFiles = 300;

        public CacheService()
        {
            if (!Directory.Exists(Constants.SongCacheFullPath)) Directory.CreateDirectory(Constants.SongCacheFullPath);
            if (!Directory.Exists(Constants.PlaylistImageCacheFullPath)) Directory.CreateDirectory(Constants.PlaylistImageCacheFullPath);
        }

        public async Task<Sprite> HandlePlaylistImageAsync(string imageUrl)
        {
            var localFilePath = Path.Combine(Constants.PlaylistImageCacheFullPath,
                Path.GetFileName(GetLocalPath(imageUrl)));

            if (File.Exists(localFilePath))
            {
                var bytes = await File.ReadAllBytesAsync(localFilePath);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return texture.ToSprite();
            }

            using (var request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                await request.SendWebRequest();
                var handler = (DownloadHandlerTexture)request.downloadHandler;
                AddToCache(localFilePath, handler.data);
                return handler.texture.ToSprite();
            }
        }

        public Task<AudioClip> HandleSongAsync(string songUrl)
        {
            try
            {
                var localFilePath = Path.Combine(Constants.SongCacheFullPath, Path.GetFileName(GetLocalPath(songUrl)));

                if (File.Exists(localFilePath)) return Task.FromResult(NAudioPlayer.FromMp3File(localFilePath));

                var songRequest = UnityWebRequest.Get(songUrl);
                songRequest.SendWebRequest();
                while (!songRequest.downloadHandler.isDone)
                {
                    //wait
                }

                var songData = songRequest.downloadHandler.data;
                AddToCache(localFilePath, songData);

                return Task.FromResult(NAudioPlayer.FromMp3File(localFilePath));
            }
            catch (Exception exception)
            {
                return Task.FromException<AudioClip>(exception);
            }
        }

        private static void AddToCache(string fullPath, byte[] bytes)
        {
            var allCacheFiles = ExtendedDirectory.GetFilesInfo(Application.temporaryCachePath, "*", SearchOption.AllDirectories)
                .OrderBy(f => f.CreationTime).ToArray();
            if (allCacheFiles.Length >= MaxNumberOfFiles)
            {
                var difference = allCacheFiles.Length - MaxNumberOfFiles;
                if (difference > 0)
                    foreach (var file in allCacheFiles.Take(difference))
                        file.Delete();

                foreach (var file in allCacheFiles.Take(50)) file.Delete();
            }

            File.WriteAllBytes(fullPath, bytes);
        }

        private static string GetLocalPath(string url) => Uri.UnescapeDataString(new Uri(url).LocalPath);
    }
}