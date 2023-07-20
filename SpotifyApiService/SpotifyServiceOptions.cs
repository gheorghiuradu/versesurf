using Microsoft.Extensions.Configuration;

namespace SpotifyApiService
{
    public class SpotifyServiceOptions
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        public SpotifyServiceOptions()
        {
        }

        public SpotifyServiceOptions(IConfiguration configuration)
        {
            configuration.GetSection(nameof(SpotifyServiceOptions)).Bind(this);
        }
    }
}