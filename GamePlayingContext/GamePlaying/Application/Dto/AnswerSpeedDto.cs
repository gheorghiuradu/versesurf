using System;

namespace GamePlaying.Application.Dto
{
    public class AnswerSpeedDto
    {
        public string Id { get; set; }
        public PlayerDto Player { get; set; }
        public string Name { get; set; }
        public DateTime ReceivedAtUTC { get; set; }
    }
}