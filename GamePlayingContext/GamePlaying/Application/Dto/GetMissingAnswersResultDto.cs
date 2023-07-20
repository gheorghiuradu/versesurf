using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class GetMissingAnswersResultDto
    {
        public IEnumerable<AnswerDto> Answers { get; set; }
        public IReadOnlyList<string> PlayerConnectionIds { get; set; }
    }
}