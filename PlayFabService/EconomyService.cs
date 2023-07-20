using PlayFab;
using PlayFab.ServerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("PlayFabService.Tests")]

namespace PlayFabService
{
    public class EconomyService
    {
        private const string PassItemClass = "Pass";
        private const string PassBundleItemClass = "PassBundle";
        private const string FrontEndLink = nameof(FrontEndLink);
        private const string BackEndLink = nameof(BackEndLink);
        private readonly PlayFabServerInstanceAPI playFabServer;
        private readonly FreeItemPolicy freeItemPolicy;

        public EconomyService(PlayFabServiceOptions options)
        {
            this.freeItemPolicy = options.FreeItemPolicy;
            this.playFabServer = new PlayFabServerInstanceAPI(new PlayFabApiSettings
            {
                TitleId = options.TitleId,
                DeveloperSecretKey = options.DeveloperSecretKey
            });
        }

        internal EconomyService(PlayFabServiceOptions options, PlayFabServerInstanceAPI playFabServerInstanceAPI)
        {
            this.freeItemPolicy = options.FreeItemPolicy;
            this.playFabServer = playFabServerInstanceAPI;
        }

        public async Task<bool> ActivateItemAsync(string playFabId, string itemInstanceId)
        {
            var itemInstance = await this.GetItemInstanceAsync(playFabId, itemInstanceId);

            if (string.Equals(itemInstance.ItemClass, PassItemClass))
            {
                return false;
            }

            await this.GrantBackendItemAsync(playFabId, itemInstance.ItemId);
            await this.ConsumeItemAsync(playFabId, itemInstance);

            return true;
        }

        public async Task ConsumeItemAsync(string playFabId, ItemInstance itemInstance)
        {
            if (itemInstance.RemainingUses > 0)
            {
                await this.playFabServer.ConsumeItemAsync(new ConsumeItemRequest
                {
                    ItemInstanceId = itemInstance.ItemInstanceId,
                    PlayFabId = playFabId,
                    ConsumeCount = 1
                });
            }
        }

        public async Task<bool> ConsumeItemAsync(string playFabId, string itemInstanceId)
        {
            var itemInstance = await this.GetItemInstanceAsync(playFabId, itemInstanceId);

            if (itemInstance.RemainingUses > 0)
            {
                await this.playFabServer.ConsumeItemAsync(new ConsumeItemRequest
                {
                    ItemInstanceId = itemInstanceId,
                    PlayFabId = playFabId,
                    ConsumeCount = 1
                });
                return true;
            }

            return false;
        }

        public async Task<bool> EnsureFreeItemsGrantedAsync(string playFabId)
        {
            // Get user data regarding free items policy from playfab user data.
            var lastFreeItemPolicyDateResult = await this.playFabServer.GetUserInternalDataAsync(new GetUserDataRequest
            {
                Keys = new List<string> { this.freeItemPolicy.PlayFabInternalDataKey },
                PlayFabId = playFabId
            });

            // Check if the free policy was ever applied
            if (lastFreeItemPolicyDateResult.Result.Data.TryGetValue(this.freeItemPolicy.PlayFabInternalDataKey, out var userDataRecord)
                && DateTime.TryParse(userDataRecord.Value, out var grantDate))
            {
                // Check when the free policy was last applied
                var daysSinceGrant = (DateTime.UtcNow - grantDate).Days;
                // If it was applied less than x days ago (specified in the policy)
                if (daysSinceGrant <= this.freeItemPolicy.Frequency)
                {
                    // Do nothing
                    return false;
                }
                // Else if items were granted more than x days (specified in the policy)
                // If user has remaining free items, remove them
                var inventoryResult = await this.playFabServer.GetUserInventoryAsync(new GetUserInventoryRequest { PlayFabId = playFabId });
                var freeItems = inventoryResult.Result.Inventory.Where(ii => ii.ItemId.Equals(this.freeItemPolicy.FreeItemId));

                if (freeItems.Any())
                {
                    await this.playFabServer.RevokeInventoryItemsAsync(new RevokeInventoryItemsRequest
                    {
                        Items = freeItems.Select(ii => new RevokeInventoryItem
                        {
                            ItemInstanceId = ii.ItemInstanceId,
                            PlayFabId = playFabId
                        }).ToList()
                    });
                }
            }

            // Grant items to the user (apply policy)
            await this.playFabServer.GrantItemsToUserAsync(new GrantItemsToUserRequest
            {
                Annotation = $"Scheduled {this.freeItemPolicy.FreeItemGrantCount} free items once every {this.freeItemPolicy.Frequency} days",
                PlayFabId = playFabId,
                ItemIds = Enumerable.Repeat(this.freeItemPolicy.FreeItemId, this.freeItemPolicy.FreeItemGrantCount).ToList()
            });

            await this.playFabServer.UpdateUserInternalDataAsync(new UpdateUserInternalDataRequest
            {
                PlayFabId = playFabId,
                Data = new Dictionary<string, string> {
                    { this.freeItemPolicy.PlayFabInternalDataKey, DateTime.UtcNow.ToString() }
                }
            });

            return true;
        }

        public async Task<PlayerProfileModel> GetPlayerProfileAsync(string playFabId)
        {
            var result = await this.playFabServer.GetPlayerProfileAsync(new GetPlayerProfileRequest
            {
                PlayFabId = playFabId,
                ProfileConstraints = new PlayerProfileViewConstraints
                {
                    ShowLocations = true
                }
            });
            return result.Result.PlayerProfile;
        }

        internal async Task<ItemInstance> GetItemInstanceAsync(string playFabId, string itemInstanceId)
        {
            var response = await this.playFabServer.GetUserInventoryAsync(new GetUserInventoryRequest { PlayFabId = playFabId });
            var inventory = response.Result?.Inventory;

            if (inventory?.Any(i => string.Equals(i.ItemInstanceId, itemInstanceId)) != true)
            {
                throw new KeyNotFoundException("Did not find the specified item in inventory");
            }

            return inventory.Find(ii => string.Equals(ii.ItemInstanceId, itemInstanceId));
        }

        internal async Task GrantBackendItemAsync(string playFabId, string frontEndItemId)
        {
            var response = await this.playFabServer.GetCatalogItemsAsync(new GetCatalogItemsRequest());
            var backEndItemId = response.Result?.Catalog.Find(ci =>
                    !string.IsNullOrWhiteSpace(ci.CustomData) &&
                    ci.CustomData.ToDictionary().Contains(new KeyValuePair<string, string>(FrontEndLink, frontEndItemId)))
                .ItemId;

            await this.playFabServer.GrantItemsToUserAsync(new GrantItemsToUserRequest
            {
                Annotation = $"Granted from front-end item id {frontEndItemId}",
                PlayFabId = playFabId,
                ItemIds = new List<string> { backEndItemId }
            });
        }

        internal async Task<CatalogItem> GetCatalogItemAsync(string itemId)
        {
            return (await this.playFabServer.GetCatalogItemsAsync(new GetCatalogItemsRequest())).Result?.Catalog?.Find(
                ci => string.Equals(ci.ItemId, itemId));
        }
    }
}