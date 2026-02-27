using GamePlaying.Domain.RoomAggregate;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Application
{
    public class DevelopmentBootstrapService
    {
        private IRoomRepository roomRepository;

        public DevelopmentBootstrapService(IRoomRepository roomRepository)
        {
            this.roomRepository = roomRepository;
        }

        public string GetLatestCode()
        {
            var room = this.roomRepository.GetTheLastRoomCreated();
            if (room == null)
            {
                return null;
            }

            return room.Code.Value;
        }
    }
}
