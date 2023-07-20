using SharedDomain.Messages.Queries;

namespace SharedDomain.Messages.Commands
{
    public class StartNewGameMessage : BaseMessage
    {
        public PlaylistOptions PlaylistOptions { get; set; }
    }
}