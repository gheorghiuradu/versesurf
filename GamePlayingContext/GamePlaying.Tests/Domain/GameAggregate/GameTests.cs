using GamePlaying.Domain;
using GamePlaying.Domain.GameAggregate;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GamePlaying.Tests.Domain.GameAggregate
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void Create_DuplicateNicks_ReturnDuplicateNicksError()
        {
            //var inputNicks = new List<string> { "nick1", "nick2", "nick1" };
            //var result = Game.Create(null, null, inputNicks);
            //Assert.IsTrue(result.IsFailure);
            //Assert.AreEqual(result.Error, Errors.Game.DuplicateNicks());
        }

        [Test]
        public void Create_UniqueNicks_ExpectCreateAPlayerForEachNick()
        {
            //var inputNicks = new HashSet<string> { "nick1", "nick2" };
            //var result = Game.Create(null, null, inputNicks);
            //Assert.IsTrue(result.IsSuccess);
            //Assert.That(result.Value.Players.Select(p => p.Nick), Is.EquivalentTo(inputNicks));
        }
    }
}
