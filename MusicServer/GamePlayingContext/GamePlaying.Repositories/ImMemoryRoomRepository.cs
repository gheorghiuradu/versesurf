using GamePlaying.Domain.RoomAggregate;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace GamePlaying.Repositories
{
    public class InMemoryRoomRepository : IRoomRepository
    {
        private readonly ConcurrentDictionary<Code, Room> rooms = new ConcurrentDictionary<Code, Room>();

        public void AddRoom(Room room)
        {
            this.rooms.TryAdd(room.Code, room);
        }

        public Room GetRoom(Code code)
        {
            this.rooms.TryGetValue(code, out var room);
            return room;
        }

        public bool CodeIsAvailable(Code code)
        {
            return !this.rooms.ContainsKey(code);
        }

        public void UpdateRoom(Room room)
        {
            //Do nothing, we have updated the object in memory
        }

        public Room GetRoomByHostConnectionId(string hostConnectionId)
        {
            return this.rooms.Values.FirstOrDefault(r => string.Equals(r.Organizer.ConnectionId, hostConnectionId));
        }

        public Room GetRoomByGuestConnectionId(string guestConnectionId)
        {
            return this.rooms.Values.FirstOrDefault(r => r.Guests.Any(g => string.Equals(g.ConnectionId, guestConnectionId)));
        }

        public void RemoveRoom(Code code)
        {
            this.rooms.TryRemove(code, out var _);
        }

        public Room GetTheLastRoomCreated()
        {
            return this.rooms.Values.LastOrDefault();
        }

        public IEnumerable<Room> GetAllRooms() => this.rooms.Values;
    }
}