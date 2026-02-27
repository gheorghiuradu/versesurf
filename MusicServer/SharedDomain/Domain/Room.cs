using SharedDomain.Domain;
using System.Collections.Generic;

namespace SharedDomain
{
    public class Room
    {
        public string Code { get; set; }
        public BookRoomRequest RoomRequest { get; set; }
        public FullPlaylistViewModel Playlist { get; set; }
        public Round CurrentRound { get; set; }
        public ScoreBoard ScoreBoard { get; set; } = new ScoreBoard();
        public List<Player> Players { get; set; } = new List<Player>();

        public void NewGame(FullPlaylistViewModel playlist)
        {
            this.CurrentRound = new Round()
            {
                Number = 1,
                Score = new ScoreBoard(this.Players)
            };
            this.ScoreBoard = new ScoreBoard(this.Players);
            this.Playlist = playlist;
        }

        public void NextRound()
        {
            this.CurrentRound = new Round
            {
                Number = this.CurrentRound?.Number + 1 ?? 1,
                Score = new ScoreBoard(this.Players)
            };
        }

        public bool KickPlayer(string playerId)
        {
            var @lock = new object();
            var foundPlayer = false;
            lock (@lock)
            {
                var player = this.Players.Find(p => string.Equals(playerId, p.Id));
                if (!(player is null))
                {
                    this.Players.Remove(player);
                    this.ScoreBoard.RemovePlayer(player);
                    this.CurrentRound.Score.RemovePlayer(player);
                    foundPlayer = true;
                }
            }

            return foundPlayer;
        }
    }

    public class BookRoomRequest
    {
        public string HostVersion { get; set; }
        public string HostPlatform { get; set; }
        public string PlayfabId { get; set; }
        public string LocaleId { get; set; }
        public List<string> AvailableCharacters { get; set; }
        public List<string> AvaialableColors { get; set; }

        public BookRoomRequest(
            List<string> availableCharacters,
            List<string> availableColors,
            string hostVersion,
            string hostPlatform,
            string playfabId,
            string localeId)
        {
            this.AvailableCharacters = availableCharacters;
            this.AvaialableColors = availableColors;
            this.HostVersion = hostVersion;
            this.HostPlatform = hostPlatform;
            this.PlayfabId = playfabId;
            this.LocaleId = localeId;
        }

        public BookRoomRequest()
        {
        }
    }

    public class Round
    {
        public int Number { get; set; } = 1;
        public ScoreBoard Score { get; set; } = new ScoreBoard();
    }
}