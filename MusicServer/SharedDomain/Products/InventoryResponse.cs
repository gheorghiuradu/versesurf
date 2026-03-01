using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SharedDomain.Products
{
    public class InventoryResponse
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public ProcessingOutcome Outcome { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProcessingOutcome
    {
        None,
        Processed,
        NotProcessed
    }
}