using Newtonsoft.Json;
using PlayFab.ClientModels;
using SharedDomain.Products;

namespace Assets.Scripts.Extensions
{
    public static class StoreItemExtensions
    {
        public static StoreItemData GetStoreItemData(this StoreItem storeItem)
        {
            return JsonConvert.DeserializeObject<StoreItemData>(storeItem.CustomData.ToString());
        }
    }
}