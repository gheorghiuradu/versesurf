using RestSharp;
using SteamWebApi.Client.Request;
using SteamWebApi.Client.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SteamWebApi.Client
{
    public class SteamWebApiClient
    {
        private const string HostAddress = "https://partner.steam-api.com/";
        private const string MicroTransactionAdress = "ISteamMicroTxn";
        private const string MicroTransactionSandboxAdress = "ISteamMicroTxnSandbox";

        private readonly IRestClient restClient;

        public SteamWebApiClient(SteamWebApiClientOptions options)
        {
            this.restClient = new RestClient(
                $"{HostAddress}{(options.UseSandbox ? MicroTransactionSandboxAdress : MicroTransactionAdress)}");
            this.restClient.AddDefaultHeader("Accept", "application/json");
            this.restClient.AddDefaultQueryParameter("key", options.ApiKey);
        }

        /// <summary>
        /// Retrieves details for a user's purchasing info.
        /// </summary>
        /// <param name="steamId">Steam ID of user making purchase.</param>
        /// <returns></returns>
        public async ValueTask<SteamResponse<GetUserInfoParams>> GetUserInfoAsync(ulong steamId)
        {
            var request = new RestRequest("GetUserInfo/v2");
            request.AddParameter(nameof(steamId), steamId);

            var response = await this.restClient.ExecuteAsync<SteamResponse<GetUserInfoParams>>(request);

            return response.Data;
        }

        /// <summary>
        /// Creates a new purchase. Send the order information along with the Steam ID to seed the transaction on Steam.
        /// </summary>
        /// <param name="initTxnRequest">Request parameters</param>
        /// <param name="items">Steam products to order</param>
        /// <returns></returns>
        public async ValueTask<SteamResponse<InitTxnParams>> InitiateTransactionAsync(InitTxnRequest initTxnRequest,
            List<Item> items)
        {
            var request = new RestRequest("InitTxn/v3", Method.POST);
            var contentDictionary = initTxnRequest.ToDictionary();

            for (int i = 0; i < items.Count; i++)
            {
                contentDictionary.Add($"itemid[{i}]", items[0].ItemId);
                contentDictionary.Add($"qty[{i}]", items[i].Qty);
                contentDictionary.Add($"amount[{i}]", items[i].Amount);
                contentDictionary.Add($"description[{i}]", items[i].Description);
                contentDictionary.Add($"category[{i}]", items[i].Category);
                if (!string.IsNullOrEmpty(items[i].BillingType))
                {
                    contentDictionary.Add($"billingtype[{i}]", items[i].BillingType);
                    contentDictionary.Add($"startdate[{i}]", items[i].StartDate);
                    contentDictionary.Add($"enddate[{i}]", items[i].EndDate);
                    contentDictionary.Add($"period[{i}]", items[i].Period);
                    contentDictionary.Add($"frequency[{i}]", items[i].Frequency);
                    contentDictionary.Add($"recurringamt[{i}]", items[i].ReccuringAmt);
                }
            }

            foreach (var pair in contentDictionary.ToList())
            {
                request.AddParameter(pair.Key, pair.Value, ParameterType.GetOrPost);
            }

            var response = await this.restClient.ExecuteAsync<SteamResponse<InitTxnParams>>(request);

            return response.Data;
        }

        /// <summary>
        /// Completes a purchase that was started by the InitTxn API.
        /// </summary>
        /// <param name="orderId">Unique 64-bit ID for order</param>
        /// <param name="appId">App ID for game.</param>
        /// <returns></returns>
        public async ValueTask<SteamResponse<InitTxnParams>> FinalizeTransactionAsync(string orderId, string appId)
        {
            var request = new RestRequest("FinalizeTxn/v2", Method.POST);
            request.AddParameter(nameof(orderId), orderId, ParameterType.GetOrPost);
            request.AddParameter(nameof(appId), appId, ParameterType.GetOrPost);

            var response = await this.restClient.ExecuteAsync<SteamResponse<InitTxnParams>>(request);

            return response.Data;
        }
    }
}