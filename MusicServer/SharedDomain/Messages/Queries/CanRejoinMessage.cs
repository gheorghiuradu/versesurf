namespace SharedDomain.Messages.Queries
{
    public class CanRejoinMessage : BaseMessage
    {
        public string GuestId { get; set; }
    }
}