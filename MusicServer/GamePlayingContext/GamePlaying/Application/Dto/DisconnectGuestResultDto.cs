namespace GamePlaying.Application.Dto
{
    public class DisconnectGuestResultDto
    {
        public string GuestId { get; set; }
        public string ActiveGameId { get; set; }
        public string HostConnectionId { get; set; }
        public string RoomCode { get; set; }
    }
}