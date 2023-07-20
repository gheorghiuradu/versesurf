using LicensingService.Models;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService
{
    internal class AscapClient
    {
        private const string BaseAddress = "https://www.ascap.com/api/wservice/MobileWeb/service/ace/api/v2.0/search/";
        private readonly RestClient client = new RestClient(BaseAddress);

        public AscapClient()
        {
            client.UseNewtonsoftJson();
        }

        public async Task<string> TryGetAscapLicenseAsync(string[] artists, string title, CancellationToken token = default)
        {
            //Processing because quirks on ascap search
            title = title.Replace("'", " ");

            foreach (var artist in artists)
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                var artistName = artist.Replace("'", " ");

                var initalRequest = this.BuildRequest(artistName, title);
                var secondRequest = this.BuildRequest(artistName, title.ReverseWords());
                var thirdRequest = this.BuildRequest(artistName.ReverseWords(), title);
                var fourthRequest = this.BuildRequest(artistName.ReverseWords(), title.ReverseWords());

                var response = await this.client.ExecuteAsync<AscapResponse>(initalRequest, token);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    response = await this.client.ExecuteAsync<AscapResponse>(secondRequest, token);
                    if (response.StatusCode == HttpStatusCode.NotFound)
                    {
                        response = await this.client.ExecuteAsync<AscapResponse>(thirdRequest, token);
                        if (response.StatusCode == HttpStatusCode.NotFound)
                        {
                            response = await this.client.ExecuteAsync<AscapResponse>(fourthRequest, token);
                            if (response.StatusCode == HttpStatusCode.NotFound)
                                continue;
                        }
                    }
                }

                var licenseResult = response.Data?.Result?.Where(r => r.TotalAscapShare > 0)
                                        .OrderByDescending(r => r.TotalAscapShare)
                                        .FirstOrDefault();

                if (licenseResult is null)
                    continue;

                return licenseResult.WorkId.ToString();
            }

            return string.Empty;
        }

        private IRestRequest BuildRequest(string artistName, string title)
        {
            return new RestRequest("title/{title}")
                .AddParameter("title", title, ParameterType.UrlSegment)
                .AddParameter("limit", 200, ParameterType.QueryString)
                .AddParameter("searchType2", "perfName", ParameterType.QueryString)
                .AddParameter("searchValue2", artistName, ParameterType.QueryString);
        }
    }
}