namespace GamePlaying.Application.Dto
{
    public class GuestRegistrationOrRejoinDto
    {
        public bool Rejoined { get; set; }
        public string HostConnectionId { get; set; }
        public PlayerDto Player { get; set; }
        public string OldConnectionId { get; internal set; }
    }
}