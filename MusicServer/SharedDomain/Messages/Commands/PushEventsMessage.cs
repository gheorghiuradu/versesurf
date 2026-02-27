using SharedDomain.InfraEvents;
using System.Collections.Generic;

namespace SharedDomain.Messages.Commands
{
    public class PushEventsMessage : BaseMessage
    {
        public IEnumerable<MusicEvent> Events { get; set; }
    }
}