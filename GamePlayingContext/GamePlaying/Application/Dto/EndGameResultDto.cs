using System.Collections.Generic;

namespace GamePlaying.Application.Dto
{
    public class EndGameResultDto
    {
        public IReadOnlyList<string> PlayerOrGuestConnectionIds { get; set; }
        public string InventoryItemId { get; set; }
        public string PlayFabId { get; set; }
        public string GameId { get; set; }
    }
}