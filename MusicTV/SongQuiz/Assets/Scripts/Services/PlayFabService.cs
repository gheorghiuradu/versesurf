using Assets.Scripts.Extensions;
using Assets.Scripts.Serialization;
using Newtonsoft.Json;
using PlayFab;
using PlayFab.ClientModels;
using SharedDomain.InfraEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

namespace Assets.Scripts.Services
{
    public class PlayFabService
    {
        private const string StoreId = "v3";
        private const string CustomEventsKey = "CustomEvents";

        public PlayFabService()
        {
            PlayFab.Internal.PlayFabWebRequest.SkipCertificateValidation();
#if UNITY_ANDROID
            // The following grants profile access to the Google Play Games SDK.
            // Note: If you also want to capture the player's Google email, be sure to add
            // .RequestEmail() to the PlayGamesClientConfiguration
            var config = new PlayGamesClientConfiguration.Builder()
            .AddOauthScope("profile")
            .RequestServerAuthCode(false)
            .Build();
            PlayGamesPlatform.InitializeInstance(config);

            // recommended for debugging:
            PlayGamesPlatform.DebugLogEnabled = true;

            // Activate the Google Play Games platform
            PlayGamesPlatform.Activate();
#endif
        }

        public async Task<string> TryLoginAsync()
        {
            var playfabId = string.Empty;

            try
            {
                var finished = false;
#if UNITY_STANDALONE
                var request = new LoginWithSteamRequest
                {
                    CreateAccount = true,
                    SteamTicket = await SteamManager.GetNewAuthSessionTicketAsync()
                };
                PlayFabClientAPI.LoginWithSteam(request, result =>
                {
                    finished = true;
                    playfabId = result.PlayFabId;
                }, error =>
                {
                    finished = true;
                    this.LogPlayFabError(error);
                });

#elif UNITY_ANDROID
                //PlayGamesPlatform.Instance.Authenticate(SignInInteractivity.CanPromptOnce, gpgResult =>
                //{
                //    if (gpgResult == SignInStatus.Success)
                //    {
                //        var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                //        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                //        {
                //            TitleId = PlayFabSettings.TitleId,
                //            ServerAuthCode = serverAuthCode,
                //            CreateAccount = true
                //        }, (result) =>
                //        {
                //            playfabId = result.PlayFabId;
                //            finished = true;
                //        }, error =>
                //        {
                //            this.LogPlayFabError(error);
                //            finished = true;
                //        });
                //    }
                //    else
                //    {
                //        Debug.LogError(gpgResult);
                //        finished = true;
                //    }
                //});
                Social.localUser.Authenticate((success, socialError) =>
                {
                    if (success)
                    {
                        var serverAuthCode = PlayGamesPlatform.Instance.GetServerAuthCode();
                        PlayFabClientAPI.LoginWithGoogleAccount(new LoginWithGoogleAccountRequest()
                        {
                            TitleId = PlayFabSettings.TitleId,
                            ServerAuthCode = serverAuthCode,
                            CreateAccount = true
                        }, (result) =>
                        {
                            playfabId = result.PlayFabId;
                            finished = true;
                        }, error =>
                        {
                            this.LogPlayFabError(error);
                            finished = true;
                        });
                    }
                    else
                    {
                        Debug.LogError(socialError);
                        finished = true;
                    }
                });
#endif
                while (!finished)
                {
                    await new WaitForSeconds(0.1f);
                }

                return playfabId;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                return playfabId;
            }
        }

        public async Task<List<StoreItem>> GetStoreItemsAsync()
        {
            List<StoreItem> storeItems = null;
            PlayFabClientAPI.GetStoreItems(new GetStoreItemsRequest
            {
                StoreId = StoreId
            }, result => storeItems = result.Store, error =>
            {
                this.LogPlayFabError(error);
                storeItems = new List<StoreItem>();
            });

            while (storeItems is null)
            {
                await new WaitForSeconds(0.001f);
            }

            return storeItems;
        }

        public async Task<List<ItemInstance>> GetInventoryItemsAsync()
        {
            List<ItemInstance> inventory = null;
            PlayFabClientAPI.GetUserInventory(new GetUserInventoryRequest(),
                result => inventory = result.Inventory, error =>
                {
                    this.LogPlayFabError(error);
                    inventory = new List<ItemInstance>();
                });

            while (inventory is null)
            {
                await new WaitForSeconds(0.001f);
            }

            return inventory;
        }

        public async Task<GameOptions> GetGameOptionsAsync()
        {
            var options = GameOptions.Default;
            var finished = false;

            PlayFabClientAPI.GetUserData(new GetUserDataRequest { Keys = options.ToDictionary().Select(kv => kv.Key).ToList() },
                result =>
                {
                    options = result.Data.ToGameOptions();
                    finished = true;
                }, error =>
                {
                    this.LogPlayFabError(error);
                    finished = true;
                });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }

            return options;
        }

        public async Task SaveGameOptionsAsync(GameOptions options)
        {
            var finished = false;
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest { Data = options.ToDictionary() },
                result => finished = true, error =>
                {
                    this.LogPlayFabError(error);
                    finished = true;
                });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }
        }

        public async Task<string> InitiatePurchaseAsync(string productId)
        {
            var currentOrderId = string.Empty;
            var finished = false;

            PlayFabClientAPI.StartPurchase(new StartPurchaseRequest()
            {
                Items = new List<ItemPurchaseRequest> {
                    new ItemPurchaseRequest
                    {
                        ItemId = productId,
                        Quantity = 1
                    }
                },
                StoreId = StoreId
            }, result =>
            {
                currentOrderId = result.OrderId;
                switch (Application.platform)
                {
                    case RuntimePlatform.WindowsEditor:
                    case RuntimePlatform.WindowsPlayer:
                    case RuntimePlatform.OSXEditor:
                    case RuntimePlatform.OSXPlayer:
                    case RuntimePlatform.LinuxEditor:
                    case RuntimePlatform.LinuxPlayer:
                        PlayFabClientAPI.PayForPurchase(new PayForPurchaseRequest
                        {
                            OrderId = result.OrderId,
                            ProviderName = "Steam",
                            Currency = "RM"
                        }, _ =>
                        {
                            Debug.Log("Initiated purchase");
                            finished = true;
                        }, error =>
                        {
                            this.LogPlayFabError(error);
                            finished = true;
                        });
                        break;
                }
            }, error =>
            {
                this.LogPlayFabError(error);
                finished = true;
            });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }

            return currentOrderId;
        }

        public async Task<IEnumerable<ItemInstance>> ConfirmPurchaseAsync(string orderId)
        {
            var finished = false;
            List<ItemInstance> items = null;
            PlayFabClientAPI.ConfirmPurchase(new ConfirmPurchaseRequest
            {
                OrderId = orderId
            }, result =>
            {
                items = result.Items;
                finished = true;
            }, error =>
            {
                this.LogPlayFabError(error);
                finished = true;
            });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }

            return items;
        }

        public async Task OpenBundleAsync(string itemInstanceId)
        {
            var finished = false;
            PlayFabClientAPI.ConsumeItem(new ConsumeItemRequest
            {
                ItemInstanceId = itemInstanceId,
                ConsumeCount = 1
            }, result => finished = true,
            error =>
            {
                this.LogPlayFabError(error);
                finished = true;
            });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }
        }

        public async Task<IEnumerable<ItemInstance>> RedeemCouponAsync(string couponCode)
        {
            IEnumerable<ItemInstance> redeemed = null;
            var finished = false;
            PlayFabClientAPI.RedeemCoupon(new RedeemCouponRequest
            {
                CouponCode = couponCode
            }, result => { redeemed = result.GrantedItems; finished = true; },
                error => { this.LogPlayFabError(error); finished = true; });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }

            return redeemed;
        }

        public async Task<bool> SaveCustomEventsAsync(IEnumerable<MusicEvent> events)
        {
            var success = false;
            var finished = false;

            var json = JsonConvert.SerializeObject(events);
            finished = false;
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
            {
                Data = new Dictionary<string, string> { { CustomEventsKey, json } }
            }, result => { finished = true; success = true; }, error => { this.LogPlayFabError(error); finished = true; });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }

            return success;
        }

        public async Task<IEnumerable<MusicEvent>> GetCustomEventsAsync()
        {
            var finished = false;
            List<MusicEvent> customEvents = null;

            PlayFabClientAPI.GetUserData(new GetUserDataRequest
            {
                Keys = new List<string> { CustomEventsKey }
            }, result =>
            {
                if (result.Data.ContainsKey(CustomEventsKey))
                {
                    customEvents = JsonConvert.DeserializeObject<List<MusicEvent>>(result.Data[CustomEventsKey].Value);
                }
                finished = true;
            }, error => { this.LogPlayFabError(error); finished = true; });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }

            if (customEvents is null)
            {
                customEvents = new List<MusicEvent>();
            }

            return customEvents;
        }

        public async Task ClearCustomEventsAsync()
        {
            var finished = false;
            PlayFabClientAPI.UpdateUserData(new UpdateUserDataRequest
            {
                KeysToRemove = new List<string> { CustomEventsKey }
            }, result => finished = true, error => { this.LogPlayFabError(error); finished = true; });

            while (!finished)
            {
                await new WaitForSeconds(0.001f);
            }
        }

        public async Task<List<TitleNewsItem>> GetTitleNewsAsync()
        {
            List<TitleNewsItem> finalResult = null;
            PlayFabClientAPI.GetTitleNews(new GetTitleNewsRequest(), result =>
            finalResult = result.News, error => { this.LogPlayFabError(error); finalResult = new List<TitleNewsItem>(); });

            while (finalResult is null)
            {
                await new WaitForSeconds(0.001f);
            }

            return finalResult;
        }

        private void LogPlayFabError(PlayFabError error)
        {
            Debug.LogError(error.GenerateErrorReport());
        }
    }
}