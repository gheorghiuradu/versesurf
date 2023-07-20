using GamePlaying.Application.Dto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamePlaying.Application.Commands
{
    public class StartNewGameCommand
    {
        public string HostConnectionId { get; set; }

        public string RoomCode { get; set; }

        public Func<Task<IEnumerable<PlaylistDto>>> GetPlaylistsAsyncTask { get; set; }
    }
}