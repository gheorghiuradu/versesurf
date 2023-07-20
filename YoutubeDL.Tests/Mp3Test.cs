using NUnit.Framework;
using System.Threading.Tasks;

namespace YoutubeDL.Tests
{
    public class Mp3Test
    {
        private YoutubeDLService youtubeService;

        [SetUp]
        public void Setup()
        {
            this.youtubeService = new YoutubeDLService();
        }

        [TestCase("https://music.youtube.com/watch?v=yO_w_2n2Pm4&feature=share")]
        [Test]
        public async Task Test1(string videoUrl)
        {
            var stream = await this.youtubeService.GetAudioStreamAsync(videoUrl);

            Assert.NotNull(stream);
        }
    }
}