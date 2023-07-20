using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class GetAllConnectedHostConnectionIdsResultDto
    {
        public IReadOnlyList<string> ConnectionIds { get; set; }
    }
}