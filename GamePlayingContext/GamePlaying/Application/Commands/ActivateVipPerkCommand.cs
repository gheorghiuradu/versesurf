using GamePlaying.Domain.RoomAggregate;

namespace GamePlaying.Application.Commands
{
    public class ActivateVipPerkCommand
    {
        public string RoomCode { get; set; }
        public string HostConnectionId { get; set; }
        public VipPerk Perk { get; set; }
    }
}