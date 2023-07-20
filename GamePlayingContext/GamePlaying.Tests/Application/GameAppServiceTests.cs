using GamePlaying.Application;
using GamePlaying.Application.Commands;
using GamePlaying.Domain.GameAggregate;
using GamePlaying.Domain.RoomAggregate;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Tests.Application
{
    [TestFixture]
    public class GameAppServiceTests
    {
        [Test]
        public void StartNewGame_Expect_RoomAndGameAreConnected()
        {
            var codeString = "ABC12";
            var hostConnectionId = Guid.NewGuid().ToString();

            var codeMock = Code.Create(codeString).Value;
            var roomMock = Room.Create(codeMock, GameSetup.Create(new List<string>(), new List<string>()).Value).Value;
            var organizerMock = Organizer.Create(hostConnectionId, "test platform", "1.0", Guid.NewGuid().ToString()).Value;
            organizerMock.BookRoom(roomMock);

            var roomRepoMock = new Mock<IRoomRepository>();
            roomRepoMock.Setup(r => r.GetRoom(codeMock)).Returns(roomMock);

            Room updatedRoom = null;
            roomRepoMock.Setup(r => r.UpdateRoom(roomMock)).Callback<Room>(r => { updatedRoom = r; });

            var gameRepoMock = new Mock<IGameRepository>();

            Game createdGame = null;
            gameRepoMock.Setup(r => r.AddGame(It.IsAny<Game>())).Callback<Game>(g => { createdGame = g; });

            var sut = new GameAppService(roomRepoMock.Object, gameRepoMock.Object);

            var actual = sut.StartNewGameAsync(new StartNewGameCommand 
            { 
                RoomCode = codeString, 
                HostConnectionId = hostConnectionId,
            });

            Assert.IsTrue(actual.IsSuccess);
            Assert.NotNull(createdGame);
            Assert.NotNull(updatedRoom);
            Assert.AreEqual(actual.Value.GameId, createdGame.Id);
            Assert.AreEqual(actual.Value.GameId, updatedRoom.GameId);
            Assert.AreEqual(roomMock.Id, createdGame.RoomId);
        }
    }
}
