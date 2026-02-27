namespace GamePlaying.Domain.Events
{
    public class GuestUpdated
    {
        public string Id { get; set; }
        public string ConnectionId { get; set; }
    }
}