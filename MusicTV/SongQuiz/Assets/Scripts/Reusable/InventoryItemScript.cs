using PlayFab.ClientModels;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class InventoryItemScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/InventoryItem";

        public string ItemInstanceId { get; internal set; }

        public Text Title;
        public GameObject CountPanel;
        public TextMeshProUGUI CountTMP;
        public TextMeshProUGUI DescriptionTMP;
        public Button Button;
        public GameObject CheckPanel;

        private void SetCount(int? remainingUses)
        {
            if (!(remainingUses is null))
            {
                this.CountTMP.text = remainingUses.ToString();
                this.CountPanel.SetActive(true);
            }
        }

        private void SetExpiration(DateTime? expiration)
        {
            if (!(expiration is null))
            {
                var timespan = expiration.GetValueOrDefault() - DateTime.UtcNow;
                this.DescriptionTMP.text
                    = $"<sprite index=0> {(timespan.Days > 0 ? timespan.Days : timespan.Hours)} {(timespan.Days > 0 ? "days" : "hours")} remaining";
                this.DescriptionTMP.gameObject.SetActive(true);
            }
        }

        private void SetActiveItem(DateTime? expiration)
        {
            if (!(expiration is null))
            {
                var timespan = expiration.GetValueOrDefault() - DateTime.UtcNow;
                this.CheckPanel.SetActive(true);
            }
        }

        private void SetOnClick(UnityAction<string> onClick)
        {
            this.OnClick.AddListener(onClick);
            this.Button.onClick.AddListener(() => this.OnClick.Invoke(this.ItemInstanceId));
        }

        public UnityEvent<string> OnClick { get; } = new OnClickStringEvent();

        public static GameObject InstantiateCustom(
            string title,
            string itemInstanceId,
            string customCount,
            Transform parent,
            UnityAction<string> onclick)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var objectInstance = GameObject.Instantiate(prefab, parent).GetComponent<InventoryItemScript>();

            objectInstance.Title.text = title;
            objectInstance.ItemInstanceId = itemInstanceId;
            objectInstance.SetOnClick(onclick);
            if (!(customCount is null))
            {
                objectInstance.CountTMP.text = customCount;
                objectInstance.CountPanel.SetActive(true);
            }

            return objectInstance.gameObject;
        }

        public static GameObject Instantiate(ItemInstance itemInstance, Transform parent, UnityAction<string> onClick)
        {
            var objectInstance = Instantiate(itemInstance, parent).GetComponent<InventoryItemScript>();
            objectInstance.SetOnClick(onClick);

            return objectInstance.gameObject;
        }

        public static GameObject Instantiate(ItemInstance itemInstance, Transform parent, bool interactable = true)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var objectInstance = GameObject.Instantiate(prefab, parent).GetComponent<InventoryItemScript>();

            objectInstance.ItemInstanceId = itemInstance.ItemInstanceId;
            objectInstance.Title.text = itemInstance.DisplayName;
            objectInstance.SetCount(itemInstance.RemainingUses);
            objectInstance.SetActiveItem(itemInstance.Expiration);
            objectInstance.SetExpiration(itemInstance.Expiration);
            objectInstance.GetComponent<Button>().interactable = interactable;

            return objectInstance.gameObject;
        }

        private class OnClickStringEvent : UnityEvent<string> { }
    }
}