namespace SharedDomain.Messages.Commands
{
    public class RemovePerkMessage : BaseMessage
    {
        public VipPerk Perk { get; set; }
    }
}