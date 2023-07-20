using Microsoft.VisualStudio.TestTools.UnitTesting;
using SignalrCoreWrapper;

namespace HubClient.Tests
{
    [TestClass]
    public class HubTests
    {
        private const string HubUrl = "https://localhost:5001/ws/gamehub";

        [TestMethod]
        public void Connect()
        {
            var client = new MusicHubClient(HubUrl);

            Assert.IsTrue(true);
        }
    }
}