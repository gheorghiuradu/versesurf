namespace GamePlaying.Application.Commands
{
    public class AskCommand
    {
        public string RoomCode { get; set; }
        public string HostConnectionId { get; set; }
        public string CorrectAnswer { get; set; }
    }
}