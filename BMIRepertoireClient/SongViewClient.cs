using HtmlAgilityPack;
using LicensingService.Models;
using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService
{
    internal class SongViewClient
    {
        private const string BaseAddress = "https://repertoire.bmi.com";
        private const int MaxPage = 10;
        private readonly HttpClient client;
        private const string Cookie = "RepInstance=instance=REP2&expires=6/15/2025 11:51:59 AM; disc=2021-06-16T16:37:28.649Z";

        public SongViewClient()
        {
            this.client = new HttpClient()
            {
                BaseAddress = new Uri(BaseAddress),
            };
            this.client.DefaultRequestHeaders.Add(nameof(Cookie), Cookie);
        }

        public async Task<SongViewResult> Search(string[] artists, string title, CancellationToken token = default)
        {
            //todo: complete
            return null;
            var page = 1;

            while (!token.IsCancellationRequested && page < MaxPage)
            {
                var result = await this.client.GetAsync(this.GetSearchQueryUrl(artists, title, page), token);

                var html = new HtmlDocument();
                html.LoadHtml(await result.Content.ReadAsStringAsync());
                var document = html.DocumentNode;

                var workNumber = this.ParseResultPage(document, artists, title);
            }
        }

        private string GetSearchQueryUrl(string[] artists, string title, int page)
        {
            return new StringBuilder("/Search/Search?SearchForm.View_Count=50&SearchForm.Main_Search=Performer&SearchForm.Main_Search_Text=")
                .AppendJoin("%20", artists)
                .Append("&SearchForm.Sub_Search=Title&SearchForm.Sub_Search_Text=")
                .Append(title)
                .Append("&SearchForm.Search_Type=all&Page_Number=").Append(page)
                .Replace(" ", "%20")
                .ToString();
        }

        private SongViewResult ParseResultPage(HtmlNode document, string[] artists, string title)
        {
            //todo: complete
            //var works = document.QuerySelectorAll("div.details-slide");
            //var relevantWorks = works.Where(w => w.QuerySelectorAll("ul.items-list").Any(n => artists.Any(a => string.Equals(a, n.InnerText, StringComparison.OrdinalIgnoreCase)))
            //                                && w.QuerySelectorAll
            return null;
        }
    }
}