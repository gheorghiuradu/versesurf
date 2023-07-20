using GamePlaying.Application.Dto;
using System;
using System.Threading.Tasks;

namespace GamePlaying.Application.Commands
{
    public class GetFullPlaylistCommand
    {
        public string HostConnectionId { get; set; }

        public string RoomCode { get; set; }

        public bool RandomRequested { get; set; }

        public Func<Task<FullPlaylistDto>> GetPlaylistAsyncTask { get; set; }

        public Func<Task<FullPlaylistDto>> GetRandomPlaylistAsyncTask { get; set; }
    }
}