using MusicStorageClient;
using NUnit.Framework;

namespace ArtManager.Tests
{
    public class MusicBrainzTests
    {
        private MockDb mockDb;
        private MusicBrainzApiClient musicBrainzApi;

        [SetUp]
        public void Setup()
        {
            MockServiceConfiguration.Build();
            this.mockDb = new MockDb();
            this.musicBrainzApi = new MusicBrainzApiClient(new GoogleStorage(new GoogleStorageOptions
            {
                BucketName = "music-storage-euw",
                PlaylistImgPrefix = "static/playlist-img",
                SongPreviewsPrefix = "static/song-previews"
            }));
        }

        [Test]
        public void If_Valid_Playlist_Return_Valid_PictureUrl()
        {
            //Arrange
            var inputPlaylist = this.mockDb.Playlists[0];

            //Act
            var imageUrl = this.musicBrainzApi.GetImageForPlaylistAsync(inputPlaylist).Result;

            //Assert
            Assert.IsNotEmpty(imageUrl);
        }
    }
}