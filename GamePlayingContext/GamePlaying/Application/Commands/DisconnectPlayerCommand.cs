namespace GamePlaying.Application.Commands
{
    public class DisconnectPlayerCommand
    {
        public string GameId { get; set; }
        public string PlayerId { get; set; }
    }
}