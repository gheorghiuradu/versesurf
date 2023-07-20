using Moq;
using NUnit.Framework;
using PlayFab;
using System;

namespace PlayFabService.Tests
{
    public class Tests
    {
        private PlayFabServiceOptions playFabServiceOptions;
        private EconomyService economyService;
        private PlayFabServerInstanceAPI playFabServerApi;

        [SetUp]
        public void Setup()
        {
            this.playFabServiceOptions = new PlayFabServiceOptions
            {
                DeveloperSecretKey = Guid.NewGuid().ToString(),
                TitleId = Guid.NewGuid().ToString(),
                FreeItemPolicy = new FreeItemPolicy
                {
                    FreeItemGrantCount = 5,
                    FreeItemId = Guid.NewGuid().ToString(),
                    Frequency = 30
                }
            };

            var playfabServerMock = new Mock<PlayFabServerInstanceAPI>();
            this.economyService = new EconomyService(this.playFabServiceOptions);
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}