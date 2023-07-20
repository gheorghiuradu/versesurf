using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService
{
    public class GMROClient
    {
        private const string BaseAddress = "https://globalmusicrights.com";
        private readonly HttpClient httpClient = new HttpClient { BaseAddress = new Uri(BaseAddress) };

        public async Task<string> GetLicenseIdAsync(string[] artists, string title, CancellationToken token = default)
        {
            throw new NotImplementedException();
            return string.Empty;
        }

        private async Task<string> PerformSearch(string artist, string title, CancellationToken token = default)
        {
            throw new NotImplementedException();
            var firstPage = await this.httpClient.GetStringAsync(this.GetSearchRequestUrl(artist, artist));

            var html = new HtmlDocument();
            html.LoadHtml(firstPage);

            var workIds = this.ParseSearchResults(html.DocumentNode, title);
            if (!workIds.Any())
            {
                foreach (var workId in workIds)
                {
                    var formDictionary = new Dictionary<string, string> { { "workId", workId } };
                    var detailsPageResponse = await this.httpClient.PostAsync("_SearchWorkDetail", new FormUrlEncodedContent(formDictionary));
                    var detailsPage = await detailsPageResponse.Content.ReadAsStringAsync();

                    var detailsHtml = new HtmlDocument();
                    detailsHtml.LoadHtml(detailsPage);

                    var isValidArtist = this.ParseDetailsPage(detailsHtml.DocumentNode, artist, title);
                }
            }
        }

        private bool ParseDetailsPage(HtmlNode documentNode, string artist, string title)
        {
            throw new NotImplementedException();
        }

        private string GetSearchRequestUrl(string artist, string title)
        {
            return Uri.EscapeDataString($@"_SearchResults?
                            filter=5&q={artist}
                            &oroperation=false
                            &filter2=2&q={title}");
        }

        private IEnumerable<string> ParseSearchResults(HtmlNode htmlNode, string title)
        {
            var results = new List<string>();
            var noResultsElements = htmlNode.QuerySelectorAll("div.search-empty");

            if (noResultsElements.Any())
            {
                return results;
            }

            var resultElements = htmlNode.QuerySelectorAll("div.search-result__item");

            var foundTitles = resultElements
                .Where(r => r.InnerText.Contains(title, StringComparison.OrdinalIgnoreCase)
                              || r.InnerText.Contains(title.ReverseWords(), StringComparison.OrdinalIgnoreCase));

            if (!foundTitles.Any())
            {
                return results;
            }

            foreach (var resultElement in foundTitles)
            {
                var tertiaryElements = resultElement.QuerySelectorAll("div.text--tertiary");
                var rightsPercentElements = tertiaryElements
                    .FirstOrDefault(e => e.InnerText.Contains("Global Music Rights represents", StringComparison.OrdinalIgnoreCase));

                if (rightsPercentElements is null)
                {
                    continue;
                }

                var rightsPercent = rightsPercentElements.InnerText
                    .Replace("Global Music Rights represents ", string.Empty)
                    .Replace("%", string.Empty);
                var percent = double.Parse(rightsPercent);
                if (percent == 0)
                {
                    continue;
                }
                var workIdElement = resultElement.QuerySelector("div.search-result__item__info__ids");

                results.Add(workIdElement.InnerText.Replace("WORK ID:", string.Empty).Trim());
            }

            return results;
        }
    }
}