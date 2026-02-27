using System.Collections.Generic;

namespace SharedDomain.Messages.Commands
{
    public class KickGuestsMessage : BaseMessage
    {
        public IEnumerable<string> GuestIds { get; set; }
    }
}