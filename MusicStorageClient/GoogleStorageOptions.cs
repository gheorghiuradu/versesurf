using GcloudWebApiExtensions;
using Microsoft.Extensions.Configuration;

namespace MusicStorageClient
{
    public class GoogleStorageOptions
    {
        public string GoogleAccountEmail { get; set; }
        public string BucketName { get; set; }
        public string SongPreviewsPrefix { get; set; }
        public string PlaylistImgPrefix { get; set; }

        public GoogleStorageOptions()
        {
        }

        public GoogleStorageOptions(IConfiguration configuration)
        {
            configuration.GetSection(nameof(GoogleStorageOptions)).Bind(this);
            this.GoogleAccountEmail = configuration.GetGcloudCredentialEmail();
        }
    }
}