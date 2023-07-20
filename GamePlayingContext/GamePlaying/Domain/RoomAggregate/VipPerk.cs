using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GamePlaying.Domain.RoomAggregate
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum VipPerk
    {
        SelectPlaylist,
        SelectCustomPlaylist,
        EditCustomPlaylist,
    }
}