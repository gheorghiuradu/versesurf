using GcloudWebApiExtensions;
using Microsoft.Extensions.Configuration;

namespace PlayFabService
{
    public class PlayFabServiceOptions
    {
        public string TitleId { get; set; }
        public string DeveloperSecretKey { get; set; }
        public FreeItemPolicy FreeItemPolicy { get; set; }

        public PlayFabServiceOptions(IConfiguration configuration, GCloudSecretProvider gCloudSecretProvider)
        {
            configuration.GetSection(nameof(PlayFabServiceOptions)).Bind(this);
            this.DeveloperSecretKey = gCloudSecretProvider.GetSecret("PlayFabVerseSurfKey");
        }
    }
}