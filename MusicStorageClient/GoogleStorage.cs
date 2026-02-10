using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MusicStorageClient
{
    public class GoogleStorage
    {
        private readonly GoogleStorageOptions options;
        private readonly WebClient web = new WebClient();

        public GoogleStorage(GoogleStorageOptions options)
        {
            this.options = options;
            options.EnsureDirectoriesExist();
        }

        public Task<string> GetPlaylistImageUrlAsync(string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, fileName);
            return Task.FromResult(filePath);
        }

        public async Task<string> UploadSongAudioAsync(Stream fileStream, string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix, fileName);
            using (var destination = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destination);
            }
            return filePath;
        }

        public async Task<string> UploadSongAudioAsync(string sourceFilePath)
        {
            var fileName = Path.GetFileName(sourceFilePath);
            var filePath = Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix, fileName);
            using (var source = File.OpenRead(sourceFilePath))
            using (var destination = new FileStream(filePath, FileMode.Create))
            {
                await source.CopyToAsync(destination);
            }
            return filePath;
        }

        public async Task<string> GetOrUploadSongPreviewAsync(string previewUrl, string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix, fileName);

            if (File.Exists(filePath))
            {
                return filePath;
            }

            var bytes = await this.web.DownloadDataTaskAsync(new Uri(previewUrl));
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

        public async Task<string> GetOrUploadSongPreviewAsync(Stream fileStream, string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix, fileName);

            if (File.Exists(filePath))
            {
                return filePath;
            }

            using (var destination = new FileStream(filePath, FileMode.Create))
            {
                await fileStream.CopyToAsync(destination);
            }
            return filePath;
        }

        public Task<Stream> DownloadFileByUrlAsync(string url)
        {
            var filePath = this.ResolveFilePath(url);
            var stream = new MemoryStream();
            using (var source = File.OpenRead(filePath))
            {
                source.CopyTo(stream);
            }
            stream.Position = 0;
            return Task.FromResult<Stream>(stream);
        }

        public Task DownloadFileByUrlAsync(string url, string destination)
        {
            var filePath = this.ResolveFilePath(url);
            File.Copy(filePath, destination, true);
            return Task.CompletedTask;
        }

        public Task<List<string>> GetAllPlaylistImagesAsync()
        {
            return Task.FromResult(GetFileListAsync(
                Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix)));
        }

        public Task<List<string>> GetAllSongPreviewsAsync()
        {
            return Task.FromResult(GetFileListAsync(
                Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix)));
        }

        public async Task<string> UploadPlaylistImageAsync(string imageUrl, string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, fileName);
            var bytes = await this.web.DownloadDataTaskAsync(new Uri(imageUrl));
            File.WriteAllBytes(filePath, bytes);
            return filePath;
        }

        public async Task<string> UploadPlaylistImageAsync(Stream imageStream, string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, fileName);
            using (var destination = new FileStream(filePath, FileMode.Create))
            {
                await imageStream.CopyToAsync(destination);
            }
            return filePath;
        }

        public async Task<string> UploadPlaylistImageAsync(string filePath)
        {
            var destPath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, Path.GetFileName(filePath));
            using (var source = File.OpenRead(filePath))
            using (var destination = new FileStream(destPath, FileMode.Create))
            {
                await source.CopyToAsync(destination);
            }
            return destPath;
        }

        public Task<bool> PlaylistImageExistsAsync(string fileName)
        {
            var filePath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, fileName);
            return Task.FromResult(File.Exists(filePath));
        }

        public Task<string> GetSignedUrlAsync(string url)
        {
            // For local storage, return the file path directly
            var filePath = this.ResolveFilePath(url);
            return Task.FromResult(filePath);
        }

        public Task DeleteObjectByUrlAsync(string url)
        {
            var filePath = this.ResolveFilePath(url);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }

        public Task<string> GetFileMd5Async(string fileName)
        {
            var filePath = this.ResolveFilePath(fileName);
            if (!File.Exists(filePath))
            {
                return Task.FromResult(string.Empty);
            }

            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                var hash = md5.ComputeHash(stream);
                return Task.FromResult(Convert.ToBase64String(hash));
            }
        }

        private List<string> GetFileListAsync(string directoryPath)
        {
            var result = new List<string>();
            if (Directory.Exists(directoryPath))
            {
                foreach (var file in Directory.GetFiles(directoryPath))
                {
                    result.Add(file);
                }
            }
            return result;
        }

        private string ResolveFilePath(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            // If it's already a local path, return as-is
            if (File.Exists(url))
            {
                return url;
            }

            // Try resolving as a relative path under base
            var underBase = Path.Combine(this.options.BasePath, url);
            if (File.Exists(underBase))
            {
                return underBase;
            }

            // Try extracting filename and look under song previews or playlist images
            var fileName = Path.GetFileName(url);
            if (!string.IsNullOrEmpty(fileName))
            {
                var songPath = Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix, fileName);
                if (File.Exists(songPath))
                {
                    return songPath;
                }

                var imgPath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, fileName);
                if (File.Exists(imgPath))
                {
                    return imgPath;
                }
            }

            // If it's a URL, try to extract the path component
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                fileName = Path.GetFileName(uri.LocalPath);
                if (!string.IsNullOrEmpty(fileName))
                {
                    var songPath = Path.Combine(this.options.BasePath, this.options.SongPreviewsPrefix, fileName);
                    if (File.Exists(songPath))
                    {
                        return songPath;
                    }

                    var imgPath = Path.Combine(this.options.BasePath, this.options.PlaylistImgPrefix, fileName);
                    if (File.Exists(imgPath))
                    {
                        return imgPath;
                    }
                }
            }

            return url;
        }
    }
}