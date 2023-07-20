using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamePlaying.Application.Commands
{
    public class GetMissingAnswersCommand
    {
        public string RoomCode { get; set; }
        public IEnumerable<string> PlayerIds { get; set; }
        public Func<Task<IEnumerable<string>>> GetAnswerTextsTask { get; set; }
    }
}