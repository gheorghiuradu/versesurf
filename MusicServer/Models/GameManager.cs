using GamePlaying.Domain.RoomAggregate;
using MusicServer.Extensions;
using SharedDomain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MusicServer.Models
{
    public class GameManager
    {
        private readonly IRoomRepository roomRepository;

        public GameManager(IRoomRepository roomRepository)
        {
            this.roomRepository = roomRepository;
        }

        public List<Game> Games { get; } = new List<Game>();
        public Random Random = new Random(DateTime.Now.Millisecond);

        public Game GetGameByCode(string codeValue)
        {
            return this.Games.Find(r => r.Code.Equals(codeValue,
            StringComparison.InvariantCultureIgnoreCase));
        }

        public Game GetGameByHostConnectionId(string hostConnectionId)
        {
            return this.Games.Find(r => r.Host.ConnectionId == hostConnectionId);
        }

        public Game GetGameByHostId(string id)
        {
            return this.Games.Find(r => r.Host.Id == id);
        }

        public ServerSidePlayer GetPlayerById(string id)
        {
            var game = this.Games.Find(g => g.Players.ContainsKey(id));
            if (game is null) return null;
            return game.Players[id];
        }

        public List<ServerSidePlayer> GetPlayersByIds(IEnumerable<string> ids)
        {
            var game = this.Games.Find(g => g.Players.Values.Any(p => string.Equals(p.Id, ids.First())));
            if (game is null) return null;
            return game.Players.Values.Where(p => ids.Any(id => string.Equals(id, p.Id))).ToList();
        }

        //public bool HostOwnsPlaylist(StringValues hostConnectionId, int id)
        //{
        //    var game = this.GetGameByHostConnectionId(hostConnectionId);

        //    return game.AvailablePlaylists.Where(p => p.Free || p.Owned).Any(p => int.Equals(p.Id, id));
        //}

        public ServerSidePlayer GetDisconnectedPlayer(string id)
        {
            var game = this.Games.Find(g => g.Players.Values.Any(p => p.Id == id && !p.IsConnected));

            if (game is null) return null;

            return game.Players.Values.FirstOrDefault(p => p.Id == id && !p.IsConnected);
        }

        public Game GetGameByPlayerId(string id)
        {
            return this.Games.Find(g => g.Players.ContainsKey(id));
        }

        public string GetNextAvailableCharacter(string gameCode)
        {
            var game = this.GetGameByCode(gameCode);
            var characterCode = string.Empty;
            var isUnique = false;

            var seal = new object();
            lock (seal)
            {
                while (!isUnique)
                {
                    //game.AvailableCharacters.Shuffle(this.Random); // Do not shuffle anymore, we asign characters sequentially for now
                    characterCode = game.AvailableCharacters.FirstOrDefault();
                    isUnique = !game.Players.Values.Any(p => p.CharacterCode.Equals(characterCode));
                }

                game.AvailableCharacters.Remove(characterCode);
            }

            return characterCode;
        }

        public Game GetGameByPlayerConnectionId(string connectionId)
        {
            return this.Games.Find(g => g.Players.Values.Any(p => string.Equals(p.ConnectionId, connectionId)));
        }

        public string GetRandomColor(string gameCode)
        {
            var game = this.GetGameByCode(gameCode);
            var color = string.Empty;
            var isUnique = false;

            var seal = new object();
            lock (seal)
            {
                while (!isUnique)
                {
                    game.AvailableColors.Shuffle(this.Random);
                    color = game.AvailableColors.FirstOrDefault();
                    isUnique = !game.Players.Values.Any(p => p.ColorCode.Equals(color));
                }

                game.AvailableColors.Remove(color);
            }

            return color;
        }

        public ServerSidePlayer GetPlayerByConnectionId(string connectionId)
        {
            var game = this.Games.Find(g => g.Players.Values.Any(p => p.ConnectionId.Equals(connectionId)));
            if (game is null) return null;

            return game.Players.Values.FirstOrDefault(p => p.ConnectionId.Equals(connectionId));
        }

        public ServerSidePlayer InitializePlayer(string code, string nickname, string connectionId)
        {
            return new ServerSidePlayer
            {
                Id = Guid.NewGuid().ToString(),
                ConnectionId = connectionId,
                Code = code,
                Nick = nickname,
                CharacterCode = this.GetNextAvailableCharacter(code),
                ColorCode = this.GetRandomColor(code),
                IsConnected = true
            };
        }

        public bool GameExistsByHostConnectionId(string hostConnectionId)
        {
            if (string.IsNullOrEmpty(hostConnectionId)) return false;
            return this.Games.Any(g => string.Equals(hostConnectionId, g.Host.ConnectionId));
        }

        public bool GameExistsByPlayerConnectionId(string playerconnectionId)
        {
            if (string.IsNullOrEmpty(playerconnectionId)) return false;
            return this.Games.Any(g => g.Players.Any(p => string.Equals(playerconnectionId, p.Value.ConnectionId)));
        }
    }
}