using Assets.Scripts.Services;
using PlayFab.ClientModels;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Store
{
    public class InventoryProductScript : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Price;
        public TextMeshProUGUI Description;

        private Button button;
        private ItemInstance product;
        private System.Action<string> onClick;

        private void Start()
        {
            this.button = this.button ?? this.GetComponent<Button>();
        }

        public void LoadItem(
            ItemInstance item,
            Action<string> onclick)
        {
            this.onClick = onclick;
            this.product = item;
            this.button = this.button ?? this.GetComponent<Button>();
            this.button.onClick.AddListener(this.OnClick);

            this.Title.text = this.product.DisplayName;
            //var price = this.product.VirtualCurrencyPrices["RM"];
            //this.Price.text = $"{price / 100f}$";
            //this.Description.text = this.product.Description;
        }

        public static GameObject Instantiate(
            Transform parent,
            ItemInstance item,
            Action<string> onclick)
        {
            var prefab = Resources.Load<GameObject>(Constants.InventoryProductButtonPrefabPath);
            var instance = GameObject.Instantiate(prefab, parent);
            instance.GetComponent<InventoryProductScript>().LoadItem(item, onclick);

            return instance;
        }

        public void OnClick()
        {
            if (!(this.onClick is null))
                this.onClick(this.product.ItemId);
        }
    }
}