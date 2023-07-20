using Microsoft.Extensions.Logging;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Linq;

namespace MusicApi.Serverless.Client
{
    public abstract class MusicApiServerlessClient
    {
        protected readonly IRestClient restClient;
        protected readonly ILogger<MusicApiServerlessClient> logger;

        public MusicApiServerlessClient(MusicApiServerlessOptions options)
        {
            this.restClient = new RestClient($"{options.EndpointUrl.Url}/api/");
            this.restClient.AddDefaultHeader("accept", "application/json");
            this.restClient.Authenticator = new GcloudAuthenticator(options.IsProduction);
            this.restClient.UseSerializer<JsonNetSerializer>();
            this.logger = options.Logger;
        }

        protected void LogIfError(IRestResponse response, IRestRequest request = null)
        {
            if (!response.IsSuccessful)
            {
                this.logger.LogError($@"Response does not indicate success
Status: {response.StatusCode}
Headers: {string.Join("; ", response.Headers.Select(h => string.Join("=", h.Name, h.Value)))}
Body: {response.Content}

URL: {response.ResponseUri}");

                if (!(request is null))
                {
                    this.logger.LogError($@"Reqeust sent
Parameters: {string.Join(";", request.Parameters.Select(p => string.Join("=", p.Name, p.Value)))}");
                }
            }
        }
    }
}