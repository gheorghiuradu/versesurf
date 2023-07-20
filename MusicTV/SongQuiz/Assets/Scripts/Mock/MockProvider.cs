using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Mock
{
    public class MockProvider
    {
        public Room FakeRoom { get; }
        public List<Answer> FakeAnswers { get; }

        public MockProvider(int players, int currentRound = 1)
        {
            this.FakeRoom = this.SetupGame(players);
            this.FakeRoom.ScoreBoard = this.GetRandomScoreBoard(currentRound);
            this.FakeRoom.CurrentRound = new SharedDomain.Round { Number = currentRound, Score = new ScoreBoard(this.FakeRoom.Players) };
            this.FakeAnswers = this.SetupAnswers();
        }

        private Room SetupGame(int players)
        {
            var code = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
            var room = new Room
            {
                Code = code,
                Players = new List<Player>()
            };

            var appsettings = ServiceProvider.Get<AppSettings>();
            room.RoomRequest = new BookRoomRequest();

            var colorCodes = appsettings.AvailableColorCodes;

            for (int i = 0; i < players; i++)
            {
                var player = new Player
                {
                    Id = Guid.NewGuid().ToString(),
                    Code = code,
                    Nick = $"FakePlayer{i}",
                    CharacterCode = appsettings.AvailableCharacters[i],
                    ColorCode = colorCodes[i]
                };
                room.Players.Add(player);
            }

            return room;
        }

        private List<Answer> SetupAnswers()
        {
            var answers = new List<Answer>();
            foreach (var player in this.FakeRoom.Players)
            {
                answers.Add(new Answer
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = $"FakeAnswer1{this.FakeRoom.Players.IndexOf(player)}",
                    Player = player,
                    Votes = new List<Vote<Answer>>()
                });
            }
            return answers;
        }

        public Vote<Answer> GetVote(Answer answer)
        {
            var i = UnityEngine.Random.Range(0, this.FakeRoom.Players.Count - 1);
            return new Vote<Answer>
            {
                By = this.FakeRoom.Players[i],
                Code = this.FakeRoom.Code,
                Item = answer
            };
        }

        //public void SetupRandomScores()
        //{
        //    this.FakeRoom.ScoreBoard = this.GetRandomScoreBoard();
        //    //ServiceProvider.AddOrReplace(this.GetRandomScoreBoard());
        //}

        public ScoreBoard GetRandomScoreBoard(int roundNumber)
        {
            var scoreBoard = new ScoreBoard(this.FakeRoom.Players);

            foreach (var player in this.FakeRoom.Players)
            {
                var score = UnityEngine.Random.Range(0, 10) % 2 == 1 ?
                    Constants.CorrectAnswerPoints : Constants.VotePoints;
                scoreBoard.AddScore(player, score * roundNumber);
            }

            return scoreBoard;
        }
    }
}