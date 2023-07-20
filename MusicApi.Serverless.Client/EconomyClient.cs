using RestSharp;
using SharedDomain.Products;
using System.Threading.Tasks;

namespace MusicApi.Serverless.Client
{
    public class EconomyClient : MusicApiServerlessClient
    {
        public EconomyClient(MusicApiServerlessOptions options) : base(options)
        {
        }

        public async ValueTask<InventoryResponse> ActivateItemAsync(string playFabId, string itemInstanceId)
        {
            var request = new RestRequest("/Inventory/ActivateItem");
            request.AddJsonBody(new InventoryRequest
            {
                ItemInstanceId = itemInstanceId,
                PlayFabId = playFabId
            });

            var response = await this.restClient.ExecutePostAsync<InventoryResponse>(request);
            this.LogIfError(response);

            return response.Data;
        }

        public async ValueTask<InventoryResponse> ConsumeItemAsync(string playFabId, string itemInstanceId)
        {
            var request = new RestRequest("/Inventory/ConsumeItem");
            request.AddJsonBody(new InventoryRequest
            {
                ItemInstanceId = itemInstanceId,
                PlayFabId = playFabId
            });

            var response = await this.restClient.ExecutePostAsync<InventoryResponse>(request);
            this.LogIfError(response);

            return response.Data;
        }

        public async ValueTask<InventoryResponse> EnsureFreeItemsPolicyAsync(string playFabId)
        {
            var request = new RestRequest("/Inventory/EnsureFreeItemsPolicy");
            request.AddJsonBody(new InventoryRequest
            {
                PlayFabId = playFabId
            });

            var response = await this.restClient.ExecutePostAsync<InventoryResponse>(request);
            this.LogIfError(response);

            return response.Data;
        }
    }
}