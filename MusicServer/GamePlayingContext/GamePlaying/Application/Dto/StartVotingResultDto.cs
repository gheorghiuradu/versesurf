using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class StartVotingResultDto
    {
        public IDictionary<string, IEnumerable<AnswerDto>> PlayerAnswerPairs { get; set; }
    }
}