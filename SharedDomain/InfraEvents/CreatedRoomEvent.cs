namespace SharedDomain.InfraEvents
{
    public class CreatedRoomEvent
    {
        public string RoomCode { get; set; }
        public string PlayFabId { get; set; }
        public string LocaleId { get; set; }
    }
}