using Assets.Scripts.Extensions;
using Assets.Scripts.Services;
using PlayFab.ClientModels;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Panels
{
    public class StoreProductScript : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Price;
        public TextMeshProUGUI Description;

        private Button button;
        private StoreItem product;
        private System.Action<string> onClick;

        private void Start()
        {
            this.button = this.button ?? this.GetComponent<Button>();
        }

        public void LoadProduct(
            StoreItem storeProduct,
            Action<string> onclick)
        {
            this.onClick = onclick;
            this.product = storeProduct;
            this.button = this.button ?? this.GetComponent<Button>();
            this.button.onClick.AddListener(this.OnClick);

            var itemData = this.product.GetStoreItemData();
            this.Title.text = itemData.DisplayName;
            var price = this.product.VirtualCurrencyPrices["RM"];
            this.Price.text = $"{price / 100f}$";
            this.Description.text = itemData.Description;
        }

        public static GameObject Instantiate(
            Transform parent,
            StoreItem storeProduct,
            Action<string> onclick)
        {
            var prefab = Resources.Load<GameObject>(Constants.StoreProductButtonPrefabPath);
            var instance = GameObject.Instantiate(prefab, parent);
            instance.GetComponent<StoreProductScript>().LoadProduct(storeProduct, onclick);

            return instance;
        }

        public void OnClick()
        {
            this.onClick(this.product.ItemId);
        }
    }
}