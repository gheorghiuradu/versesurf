using Google.Apis.Auth.OAuth2;
using Google.Apis.Iam.v1;
using Google.Apis.Services;
using Google.Apis.Storage.v1.Data;
using Google.Cloud.Storage.V1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace MusicStorageClient
{
    public class GoogleStorage
    {
        private readonly GoogleStorageOptions options;
        private readonly WebClient web = new WebClient();
        private readonly StorageClient storage;
        private readonly UrlSigner urlSigner;

        public GoogleStorage(GoogleStorageOptions options)
        {
            this.options = options;
            this.storage = StorageClient.Create();

            if (!string.IsNullOrWhiteSpace(options.GoogleAccountEmail))
            {
                var iamCredential = GoogleCredential.GetApplicationDefault().CreateScoped(IamService.Scope.CloudPlatform);
                var iamSigner = new IamServiceBlobSigner(new IamService(new BaseClientService.Initializer
                {
                    HttpClientInitializer = iamCredential
                }), options.GoogleAccountEmail);

                this.urlSigner = UrlSigner.FromBlobSigner(iamSigner);
            }
        }

        public async Task<string> GetPlaylistImageUrlAsync(string fileName)
        {
            var fileKey = $"{this.options.PlaylistImgPrefix}/{fileName}";
            var @object = await this.storage.GetObjectAsync(this.options.BucketName, fileKey);

            return @object.MediaLink;
        }

        public async Task<string> UploadSongAudioAsync(Stream fileStream, string fileName)
        {
            var fileKey = $"{this.options.SongPreviewsPrefix}/{fileName}";
            return await this.UploadFromStreamAsync(fileStream, fileKey, "audio/mpeg");
        }

        public async Task<string> UploadSongAudioAsync(string filePath)
        {
            var fileKey = $"{this.options.SongPreviewsPrefix}/{Path.GetFileName(filePath)}";
            using (var fileStream = File.OpenRead(filePath))
                return await this.UploadFromStreamAsync(fileStream, fileKey, "audio/mpeg");
        }

        public async Task<string> GetOrUploadSongPreviewAsync(string previewUrl, string fileName)
        {
            string fileUrl;
            var fileKey = $"{this.options.SongPreviewsPrefix}/{fileName}";

            try
            {
                var @object = await this.storage.GetObjectAsync(this.options.BucketName, fileKey);
                fileUrl = @object is null ?
                    (await this.UploadFromUrlAsync(previewUrl, fileKey, "audio/mpeg")) : @object.MediaLink;
            }
            catch (Exception)
            {
                fileUrl = await this.UploadFromUrlAsync(previewUrl, fileKey, "audio/mpeg");
            }

            return fileUrl;
        }

        public async Task<string> GetOrUploadSongPreviewAsync(Stream fileStream, string fileName)
        {
            string fileUrl;
            var fileKey = $"{this.options.SongPreviewsPrefix}/{fileName}";

            try
            {
                var @object = await this.storage.GetObjectAsync(this.options.BucketName, fileKey);
                fileUrl = @object is null ?
                    (await this.UploadFromStreamAsync(fileStream, fileKey, "audio/mpeg")) : @object.MediaLink;
            }
            catch (Exception)
            {
                fileUrl = await this.UploadFromStreamAsync(fileStream, fileKey, "audio/mpeg");
            }

            return fileUrl;
        }

        public async Task<Stream> DownloadFileByUrlAsync(string url)
        {
            var fileKey = this.GetFileKey(url);
            var streamDestination = new MemoryStream();
            await this.storage.DownloadObjectAsync(this.options.BucketName, fileKey, streamDestination);

            return streamDestination;
        }

        public async Task DownloadFileByUrlAsync(string url, string destination)
        {
            var fileKey = this.GetFileKey(url);
            using (var streamDestination = new FileStream(destination, FileMode.Create))
                await this.storage.DownloadObjectAsync(this.options.BucketName, fileKey, streamDestination);
        }

        public Task<List<string>> GetAllPlaylistImagesAsync()
        {
            return this.GetObjectListAsync(this.options.PlaylistImgPrefix);
        }

        public Task<List<string>> GetAllSongPreviewsAsync()
        {
            return this.GetObjectListAsync(this.options.SongPreviewsPrefix);
        }

        public Task<string> UploadPlaylistImageAsync(string imageUrl, string fileName)
        {
            var fileKey = $"{this.options.PlaylistImgPrefix}/{fileName}";
            return this.UploadFromUrlAsync(imageUrl, fileKey, "image/jpeg", true);
        }

        public Task<string> UploadPlaylistImageAsync(Stream imageStream, string fileName)
        {
            var fileKey = $"{this.options.PlaylistImgPrefix}/{fileName}";
            return this.UploadFromStreamAsync(imageStream, fileKey, "image/png", true);
        }

        public async Task<string> UploadPlaylistImageAsync(string filePath)
        {
            var fileKey = $"{this.options.PlaylistImgPrefix}/{Path.GetFileName(filePath)}";
            using (var imageStream = File.OpenRead(filePath))
                return await this.UploadFromStreamAsync(imageStream, fileKey, "image/png", true);
        }

        public async Task<bool> PlaylistImageExistsAsync(string fileName)
        {
            var fileKey = $"{this.options.PlaylistImgPrefix}/{fileName}";
            try
            {
                var fileObject = await this.storage.GetObjectAsync(this.options.BucketName, fileKey);
                return !(fileObject is null);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<string> GetSignedUrlAsync(string url)
        {
            return this.urlSigner.SignAsync(this.options.BucketName, this.GetFileKey(url), TimeSpan.FromHours(1), HttpMethod.Get);
        }

        public Task DeleteObjectByUrlAsync(string url)
        {
            return this.storage.DeleteObjectAsync(this.options.BucketName, this.GetFileKey(url));
        }

        private async Task<string> UploadFromUrlAsync(string url, string fileKey, string contentType, bool isPublic = false)
        {
            var uri = new Uri(url);
            var bytes = await this.web.DownloadDataTaskAsync(uri);
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                var uploadedObject = await storage.UploadObjectAsync(this.options.BucketName, fileKey, contentType, stream);
                if (isPublic)
                {
                    await this.MakeObjectPublic(uploadedObject);
                }
                return uploadedObject.MediaLink;
            }
        }

        private async Task<List<string>> GetObjectListAsync(string prefix = null)
        {
            var enumerator = this.storage.ListObjectsAsync(this.options.BucketName, prefix).GetAsyncEnumerator();
            var result = new List<string>();
            while (await enumerator.MoveNextAsync())
            {
                result.Add(enumerator.Current.MediaLink);
            }

            return result;
        }

        private async Task<string> UploadFromStreamAsync(Stream stream, string fileKey, string contentType, bool isPublic = false)
        {
            var uploadedObject = await storage.UploadObjectAsync(this.options.BucketName, fileKey, contentType, stream);
            if (isPublic)
            {
                await this.MakeObjectPublic(uploadedObject);
            }
            return uploadedObject.MediaLink;
        }

        private async Task MakeObjectPublic(Google.Apis.Storage.v1.Data.Object o)
        {
            o.Acl = o.Acl ?? new List<ObjectAccessControl>();
            await this.storage.UpdateObjectAsync(o, new UpdateObjectOptions
            {
                PredefinedAcl = PredefinedObjectAcl.PublicRead
            });
        }

        public async Task<string> GetFileMd5Async(string fileName)
        {
            return (await this.storage.GetObjectAsync(this.options.BucketName, this.GetFileKey(fileName))).Md5Hash;
        }

        private string GetFileKey(string url)
        {
            url = Uri.UnescapeDataString(url);

            if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) return string.Empty;

            Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri);
            switch (uri.LocalPath)
            {
                case var path when path.StartsWith($"/{this.options.BucketName}/"):
                    return path.Replace($"/{this.options.BucketName}/", "");

                case var path when path.StartsWith("/download/storage/v1/b/"):
                    return path.Replace("/download/storage/v1/b/", string.Empty)
                        .Replace($"{this.options.BucketName}/o/", string.Empty);

                case var path when path.StartsWith("/storage/v1/b/"):
                    return path.Replace("/storage/v1/b/", string.Empty)
                        .Replace($"{this.options.BucketName}/o/", string.Empty);

                default:
                    return uri.LocalPath;
            }
        }
    }
}