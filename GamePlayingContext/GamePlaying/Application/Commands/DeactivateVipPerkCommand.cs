using GamePlaying.Domain.RoomAggregate;

namespace GamePlaying.Application.Commands
{
    public class DeactivateVipPerkCommand
    {
        public string HostConnectionId { get; set; }
        public string RoomCode { get; set; }
        public VipPerk Perk { get; set; }
    }
}