using CSharpFunctionalExtensions;
using GamePlaying.Domain.Events;
using GamePlaying.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;

namespace GamePlaying.Domain.GameAggregate
{
    public class Game : Entity
    {
        public IObserver<GuestUpdated> GuestUpdatedObserver { get; }

        public string RoomId { get; }

        public HashSet<Player> Players { get; } = new HashSet<Player>();

        public string CorrectAnswerText { get; private set; }

        public Playlist Playlist { get; private set; }

        private Game(string roomId, HashSet<Player> players)
        {
            this.RoomId = roomId;
            this.Players = players;
            this.GuestUpdatedObserver = Observer.Create<GuestUpdated>(OnGuestUpdated);
        }

        public static Result<Game, Error> Create(
            string roomId,
            HashSet<Player> players)
        {
            // TODO: check params

            var game = new Game(roomId, players);

            // TODO: other game business rules

            return Result.Ok<Game, Error>(game);
        }

        public void SetPlaylist(Playlist playlist)
        {
            this.Playlist = playlist;
        }

        public void SupersedePlayerAction()
        {
            foreach (var player in Players)
            {
                player.SupersedeActions();
            }
        }

        public void SetCorrectAnswerText(string correctAnswerText)
        {
            this.CorrectAnswerText = correctAnswerText;
        }

        public void RemovePlayer(string id)
        {
            this.Players.RemoveWhere(p => string.Equals(id, p.Id));
        }

        private void OnGuestUpdated(GuestUpdated ev)
        {
            var player = this.Players.FirstOrDefault(p => p.Id.Equals(ev.Id));
            if (player == null)
            {
                return;
            }

            player.ConnectionId = ev.ConnectionId;
        }
    }
}