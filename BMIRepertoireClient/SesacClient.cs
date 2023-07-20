using LicensingService.Models;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService
{
    public class SesacClient
    {
        private const string BaseAddress = "https://api.sesac.com/website/v1/repertory/artist_search";
        private readonly RestClient client = new RestClient(BaseAddress);

        public SesacClient()
        {
            client.UseNewtonsoftJson();
        }

        public async Task<string> TryGetSesacLicenseAsync(string[] artists, string title, CancellationToken token = default)
        {
            foreach (var artist in artists)
            {
                if (token.IsCancellationRequested)
                {
                    throw new TaskCanceledException();
                }

                var initialRequest = this.BuildRequest(artist);
                var secondRequest = this.BuildRequest(artist.ReverseWords());

                var response = await this.client.ExecuteAsync<Dictionary<string, List<SESACArtistResult>>>(initialRequest, token);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    response = await this.client.ExecuteAsync<Dictionary<string, List<SESACArtistResult>>>(secondRequest, token);
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return string.Empty;
                }

                if (response.Data.ContainsKey(artist))
                {
                    return this.TryGetWorkNumber(response.Data[artist], title);
                }
                if (response.Data.ContainsKey(artist.ReverseWords()))
                {
                    return this.TryGetWorkNumber(response.Data[artist.ReverseWords()], title);
                }
            }

            return string.Empty;
        }

        private IRestRequest BuildRequest(string artist)
        {
            return new RestRequest(Method.POST).AddJsonBody(new SESACArtistSearchRequest { Term = artist });
        }

        private string TryGetWorkNumber(List<SESACArtistResult> results, string title)
        {
            return results.FirstOrDefault(r => string.Equals(r.RecordingTitle, title, StringComparison.OrdinalIgnoreCase)
            || string.Equals(r.RecordingTitle, title.ReverseWords(), StringComparison.OrdinalIgnoreCase)
            || string.Equals(r.RecordingTitle.ReverseWords(), title, StringComparison.OrdinalIgnoreCase)
            || string.Equals(r.RecordingTitle.ReverseWords(), title, StringComparison.OrdinalIgnoreCase))
            ?.WorkNumber;
        }
    }
}