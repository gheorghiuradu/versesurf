using GamePlaying.Domain.RoomAggregate;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GamePlaying.Application.Commands
{
    public class ActivateVipCommand
    {
        public string HostConnectionId { get; set; }
        public string RoomCode { get; set; }
        public string InventoryItemId { get; set; }
        public Func<string, string, ValueTask<bool>> ActivateItemAsyncTask { get; set; }
        public IEnumerable<VipPerk> DefaultVipPerks { get; set; }
    }
}