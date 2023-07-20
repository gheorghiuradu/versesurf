namespace GamePlaying.Application.Commands
{
    public class KickPlayerCommand
    {
        public string PlayerId { get; set; }
        public string RoomCode { get; set; }
        public string HostConnectionId { get; set; }
    }
}