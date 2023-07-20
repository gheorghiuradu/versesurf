using GamePlaying.Application.Dto;
using System.Collections.Generic;

namespace GamePlaying.Application.Commands
{
    public class StartVotingCommand
    {
        public string RoomCode { get; set; }
        public IEnumerable<AnswerDto> Answers { get; set; }
    }
}