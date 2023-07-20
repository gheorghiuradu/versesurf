namespace GamePlaying.Application.Dto
{
    public class RejoinPlayerResultDto
    {
        public bool Rejoined { get; set; }
        public string LatestActionName { get; set; }
        public object LatestActionParam { get; set; }
    }
}