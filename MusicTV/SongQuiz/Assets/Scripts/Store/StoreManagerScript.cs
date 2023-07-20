using Assets.Scripts.Extensions;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using Assets.Scripts.VIP;
using PlayFab.ClientModels;

#if UNITY_STANDALONE

using Steamworks;

#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Constants = Assets.Scripts.Services.Constants;

namespace Assets.Scripts.Panels
{
    public class StoreManagerScript : MonoBehaviour, IClosablePanel
    {
        public GameObject StoreContainer;
        public GameObject InventoryContainer;
        public TMP_InputField CouponCodeInput;
        public TextMeshProUGUI Error;
        public Button RedeemButton;
        public GameObject NoPass;
        public GameObject VipEffectsSpecial;

        private PlayFabService playFab;

        private List<StoreItem> store;
        private List<ItemInstance> inventory;
        private string currentOrderId;

        private List<GameObject> allProductObjects = new List<GameObject>();
#if UNITY_STANDALONE
        protected Callback<MicroTxnAuthorizationResponse_t> m_MicroTxnAuthorizationResponse;
#endif

        private void Start()
        {
            this.playFab = ServiceProvider.Get<PlayFabService>();
            ServiceProvider.Get<GameOptions>().ApplyVolume(this.gameObject);

#if UNITY_STANDALONE

            this.m_MicroTxnAuthorizationResponse =
                Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorizationResponse);
#endif
            this.CouponCodeInput.text = string.Empty;
            this.Error.text = string.Empty;
            this.CouponCodeInput.ActivateInputField();

            this.RefreshCatalog();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && this.GetComponent<RectTransform>().IsFullyVisibleFrom(Camera.main)
                && EventSystem.current.currentSelectedGameObject != this.CouponCodeInput.gameObject)
            {
                Close();
            }

            if (EventSystem.current.currentSelectedGameObject == null &&
                this.transform.IsLastChild() &&
                this.allProductObjects.Count > 0)
            {
                var firstProductButton = this.allProductObjects[0].GetComponent<Button>();
                firstProductButton.Select();
            }

            this.RedeemButton.interactable = this.CouponCodeInput.text.Length > 10;
        }

#if UNITY_STANDALONE

        private async void OnMicroTxnAuthorizationResponse(MicroTxnAuthorizationResponse_t param)
        {
            if (Convert.ToBoolean(param.m_bAuthorized))
            {
                var purchasedItems = await this.playFab.ConfirmPurchaseAsync(this.currentOrderId);
                // This is not needed for now
                //var purchasedItems = await this.playFab.ConfirmPurchaseAsync(this.currentOrderId);
                //if (!(purchasedItems is null))
                //{
                //    this.customEventService.AddEventsAsync(
                //        purchasedItems.Select(pi => new MusicEvent(SDI.EventType.PurchasedItem, pi)), true)
                //        .CatchErrors();
                //}
                if (purchasedItems?.Count() > 0)
                {
                    await this.HandleNewItems(purchasedItems);
                    this.RefreshCatalog();
                }
            }
        }

#endif

        private async void RefreshCatalog()
        {
            LoadingSpinner.Instantiate(this.transform);

            foreach (var productObject in this.allProductObjects)
            {
                GameObject.Destroy(productObject);
            }
            // When destroyed, the object becomes null so we have to clear the list
            this.allProductObjects.Clear();

            this.store = await this.playFab.GetStoreItemsAsync();
            this.inventory = await this.playFab.GetInventoryItemsAsync();

            foreach (var product in this.store)
            {
                var @object = StoreProductScript.Instantiate(
                    this.StoreContainer.transform,
                    product,
                    this.OnProductClick);
                this.allProductObjects.Add(@object);
            }

            if (this.inventory.Count > 0)
            {
                this.NoPass.SetActive(false);
                foreach (var inventoryItem in this.inventory)
                {
                    var @object = InventoryItemScript.Instantiate(
                        inventoryItem,
                        this.InventoryContainer.transform);
                    this.allProductObjects.Add(@object);
                }
            }

            var firstProductButton = this.allProductObjects[0].GetComponent<Button>();
            firstProductButton.Select();

            LoadingSpinner.Destroy();
        }

        private async Task HandleNewItems(IEnumerable<ItemInstance> purchasedItems)
        {
            var vipManager = GameObject.FindObjectOfType<VipManager>();
            if (purchasedItems.Any(ii => ii.ItemClass.EndsWith("Bundle")))
            {
                foreach (var bundle in purchasedItems.Where(ii => ii.ItemClass.EndsWith("Bundle")))
                {
                    await this.playFab.OpenBundleAsync(bundle.ItemInstanceId);
                }
            }

            if (!vipManager.IsVip)
            {
                var itemsToActivate = purchasedItems.All(ii => ii.ItemClass.EndsWith("Bundle")) ?
                        await this.playFab.GetInventoryItemsAsync() :
                        purchasedItems.Where(ii => ii.ItemClass.EndsWith("Pass") || ii.ItemClass.EndsWith("FrontEnd")).ToList();

                var item = purchasedItems.First();
                var activated = await vipManager.TryActivateItem(item);
                if (activated)
                {
                    this.VipEffectsSpecial.SetActive(true);
                    vipManager.ApplyVipAssets();
                }
            }
        }

        private async void OnProductClick(string productId)
        {
            this.currentOrderId = await this.playFab.InitiatePurchaseAsync(productId);
        }

        public void Close()
        {
            GameObject.Destroy(this.gameObject);
        }

        public async void RedeemCouponAsync()
        {
            if (string.IsNullOrWhiteSpace(this.CouponCodeInput.text))
            {
                return;
            }

            LoadingSpinner.Instantiate();
            this.RedeemButton.interactable = false;
            this.CouponCodeInput.interactable = false;

            var result = await this.playFab.RedeemCouponAsync(this.CouponCodeInput.text);

            if (result?.Count() > 0)
            {
                await this.HandleNewItems(result);
                this.CouponCodeInput.text = string.Empty;
                this.RefreshCatalog();
            }
            else
            {
                this.Error.text = "This code is invalid.";
            }

            LoadingSpinner.Destroy();
            this.RedeemButton.interactable = true;
            this.CouponCodeInput.interactable = true;
        }

        public static GameObject Instantiate()
        {
            var parent = GameObject.Find("Canvas");
            return Instantiate(parent.transform);
        }

        public static GameObject Instantiate(Transform parent)
        {
            PanelManager.CloseAllOpenPanels();
            var prefab = Resources.Load<GameObject>(Constants.StorePrefabPath);
            return GameObject.Instantiate(prefab, parent);
        }
    }
}