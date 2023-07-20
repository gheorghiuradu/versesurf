namespace GamePlaying.Application.Commands
{
    public class AnswerCommand
    {
        public string RoomCode { get; set; }
        public string PlayerId { get; set; }
        public string ConnectionId { get; set; }
        public string AnswerText { get; set; }
    }
}