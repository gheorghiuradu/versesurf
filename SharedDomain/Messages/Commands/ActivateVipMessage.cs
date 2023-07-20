using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace SharedDomain.Messages.Commands
{
    public class ActivateVipMessage : BaseMessage
    {
        public string InventoryItemId { get; set; }
        public List<VipPerk> DefaultPerks { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum VipPerk
    {
        SelectPlaylist,
        SelectCustomPlaylist,
        EditCustomPlaylist,
    }
}