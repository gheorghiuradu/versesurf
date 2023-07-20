using NUnit.Framework;

namespace LicensingService.Tests
{
    [TestFixture]
    public class GMROClientTests
    {
        private GMROClient client;

        [SetUp]
        public void Setup()
        {
            this.client = new GMROClient();
        }

        //[Test]
        //public void If_Valid_Input_Should_Return_Valid_LicenseId()
        //{
        //    // arrange
        //    var artists = new[] { "ava max" };
        //    var title = "sweet but psycho";

        //    //act
        //    var licenseId = this.client.GetLicenseIdAsync(artists, title).Result;

        //    //assert
        //    Assert.IsNotEmpty(licenseId);
        //}
    }
}