using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;

namespace MusicServer.Words
{
    public class WordProvider
    {
        private readonly IWebHostEnvironment env;

        public WordProvider(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public async ValueTask<string> GetRandomWordAsync()
        {
            var json = await File.ReadAllTextAsync(Path.Combine(this.env.ContentRootPath, "Words", "words.json"));
            var words = JsonConvert.DeserializeObject<string[]>(json);
            var random = new Random((int)DateTime.Now.Ticks).Next(0, words.Length - 1);

            return words[random];
        }
    }
}