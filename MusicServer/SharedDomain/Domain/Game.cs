using SharedDomain.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace SharedDomain
{
    public class Game
    {
        public string Code { get; set; }
        public Host Host { get; set; }
        public string CurrentAnswer { get; set; }
        public List<PlaylistViewModel> AvailablePlaylists { get; set; }
        public List<string> AvailableCharacters { get; set; }
        public List<string> AvailableColors { get; set; }
        public ConcurrentDictionary<string, ServerSidePlayer> Players { get; set; }
        public List<string> OrderIds { get; set; }

        public bool ContainsNick(string nick)
        {
            if (this.Players is null) return false;

            return this.Players.Values.Select(p => p.Nick).Contains(nick);
        }

        public void AddAction(string name, object param = null)
        {
            foreach (var player in this.Players.Values)
            {
                player.AddAction(name, param);
            }
        }

        public void AddAction(IEnumerable<ServerSidePlayer> players, string name, object param = null)
        {
            foreach (var player in players)
            {
                player.AddAction(name, param);
            }
        }

        public void Supersede()
        {
            foreach (var player in this.Players.Values)
            {
                player.Supersede();
            }
        }

        public void Supersede(IEnumerable<ServerSidePlayer> players)
        {
            foreach (var item in players)
            {
                item.Supersede();
            }
        }

        public void Supersede(string playerId)
        {
            this.Players[playerId]?.Supersede();
        }

        public Game(BookRoomRequest request, string connectionId, string code)
        {
            this.Code = code;
            this.Players = new ConcurrentDictionary<string, ServerSidePlayer>();
            this.Host = new Host
            {
                Id = Guid.NewGuid().ToString(),
                Code = code,
                ConnectionId = connectionId,
                Version = request.HostVersion,
                Platform = request.HostPlatform,
                UserId = request.PlayfabId
            };
            this.AvailableCharacters = request.AvailableCharacters;
            this.AvailableColors = request.AvaialableColors;
            this.OrderIds = new List<string>();
        }

        //public Room CreateRoom(RoomRequest request)
        //{
        //    return new Room
        //    {
        //        Players = new List<Player>(),
        //        RoomRequest = request,
        //        Code = this.Code,
        //        CurrentRound = 0,
        //        ScoreBoard = new ScoreBoard()
        //    };
        //}

        public void RemoveDisconnectedPlayers()
        {
            var disconnected = this.Players.Values.Where(p => !p.IsConnected);
            if (disconnected.Any())
            {
                var connected = this.Players.Values.Except(disconnected);
                this.Players.Clear();
                foreach (var player in connected)
                {
                    this.Players.TryAdd(player.Id, player);
                }
            }
        }
    }
}