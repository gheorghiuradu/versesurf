using NUnit.Framework;
using System.Threading.Tasks;

namespace MusicStorageClient.Tests
{
    public class GoogleStorageTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [TestCase("https://theinterwission.ro/wp-content/uploads/2016/09/bpc.jpg", "BPC")]
        public async Task UploadPlaylistImage(string imageUrl, string fileName)
        {
            var sut = new GoogleStorage(new GoogleStorageOptions
            {
                BucketName = "music-storage-euw",
                PlaylistImgPrefix = "static/playlist-img",
                SongPreviewsPrefix = "static/song-previews"
            });

            await sut.UploadPlaylistImageAsync(imageUrl, fileName);

            Assert.Pass();
        }
    }
}