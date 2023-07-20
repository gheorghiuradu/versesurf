using System.Collections.Generic;

namespace GamePlaying.Domain.RoomAggregate
{
    public interface IRoomRepository
    {
        void AddRoom(Room room);

        Room GetRoom(Code code);

        Room GetTheLastRoomCreated();

        void UpdateRoom(Room room);

        bool CodeIsAvailable(Code code);

        Room GetRoomByHostConnectionId(string hostConnectionId);

        Room GetRoomByGuestConnectionId(string guestConnectionId);

        void RemoveRoom(Code code);

        IEnumerable<Room> GetAllRooms();
    }
}