using Newtonsoft.Json;
using System.Collections.Generic;

namespace SteamWebApi.Client.Request
{
    public class InitTxnRequest
    {
        /// <summary>
        /// Unique 64-bit ID for order
        /// </summary>
        public string OrderId { get; set; }

        /// <summary>
        /// Steam ID of user making purchase.
        /// </summary>
        public string SteamId { get; set; }

        /// <summary>
        /// App ID of game this transaction is for.
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// Number of items in cart.
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// ISO 639-1 language code of the item descriptions.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// ISO 4217 currency code.
        /// </summary>
        public string Currency { get; set; }

        public IDictionary<string, object> ToDictionary()
        {
            var json = JsonConvert.SerializeObject(this);
            return JsonConvert.DeserializeObject<IDictionary<string, object>>(json);
        }
    }

    public class Item
    {
        /// <summary>
        /// 3rd party ID for item.
        /// </summary>
        public string ItemId { get; set; }

        /// <summary>
        /// Quantity of this item.
        /// </summary>
        public int Qty { get; set; }

        /// <summary>
        /// Total cost (in cents) of item(s) to be charged at this time.
        /// </summary>
        public double Amount { get; set; }

        /// <summary>
        /// Description of item.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Optional category grouping for item.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Optional recurring billing type.
        /// </summary>
        public string BillingType { get; set; }

        /// <summary>
        /// Optional start date for recurring billing.
        /// </summary>
        public string StartDate { get; set; }

        /// <summary>
        /// Optional end date for recurring billing.
        /// </summary>
        public string EndDate { get; set; }

        /// <summary>
        /// Optional period for recurring billing.
        /// </summary>
        public string Period { get; set; }

        /// <summary>
        /// Optional frequency for recurring billing.
        /// </summary>
        public string Frequency { get; set; }

        /// <summary>
        /// Optional amount to be billed for future recurring billing transactions.
        /// </summary>
        public float ReccuringAmt { get; set; }
    }
}