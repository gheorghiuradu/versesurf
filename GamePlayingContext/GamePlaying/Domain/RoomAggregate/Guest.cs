using CSharpFunctionalExtensions;
using GamePlaying.Infrastructure;

namespace GamePlaying.Domain.RoomAggregate
{
    public class Guest : Entity
    {
        public Room Room { get; set; }
        public string Nick { get; set; }
        public Color Color { get; }
        public Character Character { get; }
        public string ConnectionId { get; set; }
        public bool IsConnected { get; private set; }

        public Guest(Room room,
            string nick,
            Color color,
            Character character,
            string connectionId)
        {
            this.Nick = nick;
            this.Character = character;
            this.Color = color;
            this.ConnectionId = connectionId;
            this.IsConnected = true;
        }

        public static Result<Guest, Error> Create(
            Room room,
            string nick,
            Color color,
            Character emoji,
            string connectionId)
        {
            // Validate data
            var player = new Guest(room, nick, color, emoji, connectionId);

            // TODO: other code business rules

            return Result.Ok<Guest, Error>(player);
        }

        internal void Disconnect()
        {
            this.IsConnected = false;
        }

        internal void Connect()
        {
            this.IsConnected = true;
        }
    }
}