using Assets.Scripts.Extensions;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Assets.Scripts.Services
{
    public class CacheService
    {
        private const int MaxNumberOfFiles = 300;
        private readonly MusicClient musicClient;

        public CacheService(MusicClient musicClient)
        {
            this.musicClient = musicClient;
            if (!Directory.Exists(Constants.SongCacheFullPath))
            {
                Directory.CreateDirectory(Constants.SongCacheFullPath);
            }
            if (!Directory.Exists(Constants.PlaylistImageCacheFullPath))
            {
                Directory.CreateDirectory(Constants.PlaylistImageCacheFullPath);
            }
        }

        public async Task<Sprite> HandlePlaylistImageAsync(string imageUrl, string hash)
        {
            var localFilePath = Path.Combine(Constants.PlaylistImageCacheFullPath,
                    Path.GetFileName(this.GetLocalPath(imageUrl)));

            if (this.HasValidFile(localFilePath, hash))
            {
                var bytes = File.ReadAllBytes(localFilePath);
                var texture = new Texture2D(2, 2);
                texture.LoadImage(bytes);
                return texture.ToSprite();
            }

            using (var request = UnityWebRequestTexture.GetTexture(imageUrl))
            {
                await request.SendWebRequest();
                var handler = ((DownloadHandlerTexture)request.downloadHandler);
                this.AddToCache(localFilePath, handler.data);
                return handler.texture.ToSprite();
            }
        }

        public void DownloadSong(string songUrl, string hash)
        {
            var escapedUrl = Uri.EscapeDataString(songUrl);
            var localFilePath = Path.Combine(Constants.SongCacheFullPath, Path.GetFileName(this.GetLocalPath(songUrl)));

            if (this.HasValidFile(localFilePath, hash))
            {
                return;
            }

            var songReuqest = UnityWebRequest.Get(escapedUrl);
            songReuqest.SendWebRequest();
            while (!songReuqest.downloadHandler.isDone)
            {
                //wait
            }

            var songData = songReuqest.downloadHandler.data;
            this.AddToCache(localFilePath, songData);
        }

        public async Task<AudioClip> HandleSongAsync(string songUrl, string hash)
        {
            var escapedUrl = Uri.EscapeDataString(songUrl);
            var localFilePath = Path.Combine(Constants.SongCacheFullPath, Path.GetFileName(this.GetLocalPath(songUrl)));

            if (this.HasValidFile(localFilePath, hash))
            {
                return NAudioPlayer.FromMp3File(localFilePath);
            }

            var songData = await this.musicClient.DownloadSongAsync(escapedUrl);
            this.AddToCache(localFilePath, songData);

            return NAudioPlayer.FromMp3File(localFilePath);
        }

        private bool HasValidFile(string filePath, string md5)
        {
            if (File.Exists(filePath))
            {
                string hash64;
                using (var md5Service = MD5.Create())
                {
                    var fileBytes = File.ReadAllBytes(filePath);
                    var md5Bytes = md5Service.ComputeHash(fileBytes);

                    hash64 = Convert.ToBase64String(md5Bytes);
                }

                return string.Equals(hash64, md5, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private void AddToCache(string fullPath, byte[] bytes)
        {
            var allCacheFiles = ExtendedDirectory.GetFilesInfo(Application.temporaryCachePath, "*", SearchOption.AllDirectories)
                .OrderBy(f => f.CreationTime);
            if (allCacheFiles.Count() >= MaxNumberOfFiles)
            {
                var difference = allCacheFiles.Count() - MaxNumberOfFiles;
                if (difference > 0)
                {
                    foreach (var file in allCacheFiles.Take(difference))
                    {
                        file.Delete();
                    }
                }
                foreach (var file in allCacheFiles.Take(50))
                {
                    file.Delete();
                }
            }

            File.WriteAllBytes(fullPath, bytes);
        }

        private string GetLocalPath(string url)
        {
            return Uri.UnescapeDataString(new Uri(url).LocalPath);
        }
    }
}