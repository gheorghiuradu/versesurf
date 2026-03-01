namespace GamePlaying.Application.Commands
{
    public class RecordVoteCommand
    {
        public string PlayerId { get; set; }
        public string RoomCode { get; set; }
        public string ConnectionId { get; set; }
    }
}