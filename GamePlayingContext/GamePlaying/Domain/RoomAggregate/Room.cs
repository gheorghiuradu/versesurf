using CSharpFunctionalExtensions;
using GamePlaying.Domain.Events;
using GamePlaying.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;

namespace GamePlaying.Domain.RoomAggregate
{
    public class Room : Entity, IDisposable
    {
        public Subject<GuestUpdated> GuestUpdatedObservable { get; }

        private Room(Code code, GameSetup gameSetup)
        {
            this.Code = code;
            this.GameSetup = gameSetup;
            this.GuestUpdatedObservable = new Subject<GuestUpdated>();
        }

        private Room(Code code, GameSetup gameSetup, HashSet<Guest> players)
        {
            this.Code = code;
            this.GameSetup = gameSetup;
            this.Guests = players;
        }

        public virtual Code Code { get; }

        public GameSetup GameSetup { get; }

        public virtual Organizer Organizer { get; set; }

        public HashSet<Guest> Guests { get; } = new HashSet<Guest>();

        public string GameId { get; private set; }

        private IList<Color> AvailableColors =>
            this.GameSetup.Colors.Where(c => this.ColorIsAvailable(c)).ToList();

        public IList<Character> AvailableCharacters =>
            this.GameSetup.Characters.Where(e => this.CharacterIsAvailable(e)).ToList();

        public static Result<Room, Error> Create(Code code, GameSetup gameSetup)
        {
            var room = new Room(code, gameSetup);

            // TODO: other room business rules

            return Result.Ok<Room, Error>(room);
        }

        internal static Result<Room, Error> Create(Code code, GameSetup gameSetup, HashSet<Guest> players)
        {
            var room = new Room(code, gameSetup, players);

            // TODO: other room business rules

            return Result.Ok<Room, Error>(room);
        }

        internal void SetCurrentGame(string gameId, IObserver<GuestUpdated> guestUpdatedObserver)
        {
            this.GameId = gameId;
            this.GuestUpdatedObservable.Subscribe(guestUpdatedObserver);
        }

        internal void EndCurrentGame()
        {
            this.GameId = string.Empty;
        }

        internal bool IsFull()
        {
            return this.Guests.Count >= 8;
        }

        internal bool ContainsGuest(string guestId)
        {
            return this.Guests.Any(g => g.Id.Equals(guestId));
        }

        internal bool NickIsTaken(string guestNick, string guestId)
        {
            return this.Guests.Any(g => g.Nick.Equals(guestNick) && !g.Id.Equals(guestId));
        }

        internal bool NickIsTaken(string guestNick)
        {
            return this.Guests.Any(g => g.Nick.Equals(guestNick));
        }

        internal bool IsGameInProgress() => !string.IsNullOrEmpty(this.GameId);

        internal Guest RegisterGuest(string nick, string connectionId)
        {
            var guestResult = Guest.Create(
                this,
                nick,
                this.GetRandomColor(),
                this.GetRandomEmoji(),
                connectionId);

            if (guestResult.IsFailure)
            {
                // TODO: log
            }

            var guest = guestResult.Value;
            this.Guests.Add(guest);

            return guest;
        }

        public Color GetRandomColor()
        {
            Color color;

            do
            {
                this.AvailableColors.Shuffle();
                color = this.AvailableColors.FirstOrDefault();
            }
            while (this.ColorIsTaken(color));

            return color;
        }

        private bool ColorIsTaken(Color color)
        {
            return this.Guests.Any(g => g.Color.Equals(color));
        }

        private bool ColorIsAvailable(Color color)
        {
            return !this.ColorIsTaken(color);
        }

        public Character GetRandomEmoji()
        {
            Character character;

            do
            {
                this.AvailableCharacters.Shuffle();
                character = this.AvailableCharacters.FirstOrDefault();
            }
            while (this.CharacterIsTaken(character));

            return character;
        }

        private bool CharacterIsTaken(Character character)
        {
            return this.Guests.Any(g => g.Character.Equals(character));
        }

        private bool CharacterIsAvailable(Character character)
        {
            return !this.CharacterIsTaken(character);
        }

        public void NotifyGuestUpdated(GuestUpdated guestUpdated)
        {
            this.GuestUpdatedObservable.OnNext(guestUpdated);
        }

        public void TryRemoveGuest(string guestId)
        {
            if (this.IsGameInProgress())
            {
                throw new InvalidOperationException("Cannot remove guests while a game is in progres.");
            }
            this.Guests.RemoveWhere(g => string.Equals(guestId, g.Id));
        }

        public void RemoveGuest(string guestId)
        {
            this.Guests.RemoveWhere(g => string.Equals(guestId, g.Id));
        }

        public void Dispose()
        {
            this.GuestUpdatedObservable.Dispose();
        }
    }
}