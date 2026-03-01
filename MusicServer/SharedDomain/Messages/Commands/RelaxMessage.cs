using System.Collections.Generic;

namespace SharedDomain.Messages.Commands
{
    public class RelaxMessage : BaseMessage
    {
        public IEnumerable<string> PlayerOrGuestIds { get; set; }
    }
}