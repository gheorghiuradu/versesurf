using Fizzler.Systems.HtmlAgilityPack;
using HtmlAgilityPack;
using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LicensingService
{
    internal class BMIClient
    {
        private const string BaseAddress = "https://repertoire.bmi.com";
        private const int MaxPage = 10;
        private readonly HttpClient client;
        private const string Cookie = "RepInstance=instance=REP2&expires=6/15/2025 11:51:59 AM; disc=2021-06-16T16:37:28.649Z";

        public BMIClient()
        {
            this.client = new HttpClient()
            {
                BaseAddress = new Uri(BaseAddress),
            };
            this.client.DefaultRequestHeaders.Add(nameof(Cookie), Cookie);
        }

        public async Task<string> TryGetBMIWorkNumberAsync(string[] artists, string title, CancellationToken token = default)
        {
            //todo replace this with songviewclient
            return null;
            //var page = 1;

            //while (!token.IsCancellationRequested && page < MaxPage)
            //{
            //    var result = await this.client.GetAsync(this.GetSearchQueryUrl(artists, title, page), token);

            //    var html = new HtmlDocument();
            //    html.LoadHtml(await result.Content.ReadAsStringAsync());
            //    var document = html.DocumentNode;

            //    var workNumber = this.ParseResultPage(document, artists, title);

            //    switch (currentSearchType)
            //    {
            //        case SearchType.title:
            //            workNumber = this.ParseTitleResultPage(document, artists);
            //            break;

            //        case SearchType.artist:
            //            workNumber = await this.ParseArtistResultsAsync(document, currentArtist, title);
            //            break;
            //    }

            //    if (!string.IsNullOrEmpty(workNumber))
            //    {
            //        return workNumber;
            //    }
            //    else if (page > MaxPage)
            //    {
            //        if (currentSearchType == SearchType.artist
            //            && Array.IndexOf(artists, currentArtist) < artists.Length - 1)
            //        {
            //            currentArtist = artists[Array.IndexOf(artists, currentArtist) + 1];
            //            ResetPaging();
            //        }
            //        else if (step < 2)
            //        {
            //            step++;
            //            this.currentSearchType = currentSearchType.Next();
            //            ResetPaging();
            //        }
            //        else
            //        {
            //            return workNumber;
            //        }
            //    }
            //    else
            //    {
            //        var nextPageElem = document.SelectSingleNode("//a[.='Next>']");
            //        if (nextPageElem is null)
            //        {
            //            if (step < 2)
            //            {
            //                step++;
            //                this.currentSearchType = currentSearchType.Next();
            //                ResetPaging();
            //            }
            //            else
            //            {
            //                return workNumber;
            //            }
            //        }
            //        else
            //        {
            //            page++;
            //            fromRow += toRow;
            //            toRow += 25;
            //        }
            //    }
            //}

            //return workNumber;
        }

        private string ParseResultPage(HtmlNode document, string[] artists, string title)
        {
            //todo: complete
            return null;
        }

        private async Task<string> ParseArtistResultsAsync(HtmlNode document, string artist, string title)
        {
            var workNumber = string.Empty;

            var artistElems = document.QuerySelectorAll("td > a");
            var foundArtists = artistElems
                .Select(e => e.InnerText)
                .Where(pa => pa.Equals(artist, StringComparison.OrdinalIgnoreCase)
                                               || pa.Equals(artist.ReverseWords(), StringComparison.OrdinalIgnoreCase));

            if (foundArtists.Any())
            {
                foreach (var foundArtist in foundArtists)
                {
                    var artistUrl = artistElems.FirstOrDefault(e => e.InnerText == foundArtist)
                        .GetAttributeValue<string>("href", string.Empty);
                    workNumber = await ParseWorkPagesAsync(artistUrl, title);

                    if (string.IsNullOrEmpty(workNumber)) return workNumber;
                }
            }

            return workNumber;
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

        private string ParseTitleResultPage(HtmlNode document, string[] artists)
        {
            var workNumber = string.Empty;

            var artistElems = document.QuerySelectorAll("td.entity");
            var pageArtists = artistElems.Select(e => e.InnerText);
            var foundArtist = pageArtists.FirstOrDefault(pa => artists.Any(artist => pa.Equals(artist, StringComparison.OrdinalIgnoreCase))
                                                            || artists.Any(artist => pa.Equals(artist.ReverseWords(), StringComparison.OrdinalIgnoreCase)));

            if (!(foundArtist is null))
            {
                var artistElement = artistElems.FirstOrDefault(e => e.InnerText == foundArtist);
                var workTable = artistElement.ParentNode.ParentNode.ParentNode.PreviousSibling;

                var totalControlsText = workTable
                    .QuerySelector("td.work_number")
                    .InnerText
                    .Replace("Total Controlled by BMI:", string.Empty)
                    .Replace("%", string.Empty)
                    .Trim();
                var percentOwned = double.Parse(totalControlsText);

                if (percentOwned > 0)
                {
                    workNumber = workTable.InnerText.Split('\n')
                    .FirstOrDefault(s => s.Contains("BMI Work #"))
                    .Replace("BMI Work #", "")
                    .Trim();
                }
            }

            return workNumber;
        }

        private async Task<string> ParseWorkPagesAsync(string pageUrl, string title)
        {
            var workNumber = string.Empty;
            var html = new HtmlDocument();

            while (true)
            {
                var htmlString = await client.GetStringAsync(pageUrl);

                html.LoadHtml(htmlString);

                var cells = html.DocumentNode.QuerySelectorAll("td");
                var workcell = cells.FirstOrDefault(e => e.InnerText.Equals(title, StringComparison.OrdinalIgnoreCase));
                if (!(workcell is null))
                {
                    workNumber = workcell.NextSibling.InnerText;
                    return workNumber;
                }
                var nextPageElem = html.DocumentNode.SelectSingleNode("//a[.='Next>']");
                if (!(nextPageElem is null))
                {
                    pageUrl = nextPageElem.GetAttributeValue<string>("href", string.Empty);
                }
                else
                {
                    return workNumber;
                }
            }
        }
    }
}