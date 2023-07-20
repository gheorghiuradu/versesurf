using NUnit.Framework;

namespace LicensingService.Tests
{
    public class AscapClientTests
    {
        private AscapClient ascapClient;

        [SetUp]
        public void Setup()
        {
            this.ascapClient = new AscapClient();
        }

        [Test]
        public void Valid_Artist_Title_Should_Return_Valid_WorkId_1()
        {
            //Arrange
            var artists = new[] { "michael jackson" };
            var title = "don't matter to me";

            //Act
            var license = this.ascapClient.TryGetAscapLicenseAsync(artists, title).Result;

            //Assert
            Assert.IsNotEmpty(license);
        }

        [Test]
        public void Valid_Artist_Title_Should_Return_Valid_WorkId_2()
        {
            //Arrange
            var artists = new[] { "joe cocker" };
            var title = "bad bad sign";

            //Act
            var license = this.ascapClient.TryGetAscapLicenseAsync(artists, title).Result;

            //Assert
            Assert.IsNotEmpty(license);
        }
    }
}