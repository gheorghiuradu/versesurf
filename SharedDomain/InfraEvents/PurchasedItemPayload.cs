using System;

namespace SharedDomain.InfraEvents
{
    public class PurchasedItemPayload
    {
        public string ItemId { get; set; }
        public string ItemInstanceId { get; set; }
        public string PlayFabId { get; set; }
        public string Currency { get; set; }
        public uint Price { get; set; }
        public DateTime? PurchaseDate { get; set; }
    }
}