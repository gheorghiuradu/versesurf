using System.Collections.Generic;

namespace SharedDomain.Messages.Queries
{
    public class GetMissingAnswersMessage : BaseMessage
    {
        public IEnumerable<string> PlayerIds { get; set; }
    }
}