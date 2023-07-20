using System.Collections.Generic;

namespace GamePlaying.Application.Commands
{
    public class BookRoomCommand
    {
        public string HostConnectionId { get; set; }
        public string HostPlatform { get; set; }
        public string HostVersion { get; set; }
        public string OrganizerPlayfabId { get; set; }
        public List<string> GameSetupAvailableColors { get; set; }
        public List<string> GameSetupAvailableCharacters { get; set; }
        public string LocaleId { get; set; }
    }
}