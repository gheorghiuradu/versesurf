namespace SharedDomain.Messages.Commands
{
    public class KickPlayerMessage : BaseMessage
    {
        public string PlayerId { get; set; }
    }
}