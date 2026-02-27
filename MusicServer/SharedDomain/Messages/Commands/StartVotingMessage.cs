using System.Collections.Generic;

namespace SharedDomain.Messages.Commands
{
    public class StartVotingMessage : BaseMessage
    {
        public IEnumerable<Answer> Answers { get; set; }
    }
}