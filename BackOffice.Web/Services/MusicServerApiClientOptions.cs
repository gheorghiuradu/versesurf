using Microsoft.Extensions.Configuration;

namespace BackOffice.Web.Services
{
    public class MusicServerApiClientOptions
    {
        public string GameControllerUrl { get; set; }

        public MusicServerApiClientOptions(IConfiguration config)
        {
            config.GetSection(nameof(MusicServerApiClientOptions)).Bind(this);
        }
    }
}