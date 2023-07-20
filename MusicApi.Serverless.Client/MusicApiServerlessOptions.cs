using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace MusicApi.Serverless.Client
{
    public class MusicApiServerlessOptions
    {
        public EndpointUrl EndpointUrl { get; set; }
        public bool IsProduction { get; set; }
        public ILogger<MusicApiServerlessClient> Logger { get; set; }
        public bool Enabled { get; set; }

        public MusicApiServerlessOptions(
            EndpointUrl endpointUrl,
            ILogger<MusicApiServerlessClient> logger,
            IHostingEnvironment hostingEnvironment)
        {
            this.EndpointUrl = endpointUrl;
            this.Logger = logger;
            this.IsProduction = !hostingEnvironment.IsDevelopment();
            this.Enabled = !hostingEnvironment.IsDevelopment();
        }
    }

    public class EndpointUrl
    {
        public string Url { get; set; }

        public EndpointUrl(string url)
        {
            this.Url = url;
        }

        public EndpointUrl()
        {
        }
    }
}