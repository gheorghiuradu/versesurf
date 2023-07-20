using NUnit.Framework;

namespace LicensingService.Tests
{
    public class BMIClientTests
    {
        private BMIClient bMIClient;

        [SetUp]
        public void Setup()
        {
            this.bMIClient = new BMIClient();
        }

        [Test]
        public void If_Valid_Input_Should_Return_ValidLicense()
        {
            //Arrange
            var artists = new[] { "2Pac" };
            var title = "Dear mama";

            //Act
            var licenseId = this.bMIClient.TryGetBMIWorkNumberAsync(artists, title).Result;

            //Assert
            Assert.IsNotEmpty(licenseId);
        }
    }
}