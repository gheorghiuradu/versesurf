using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Reusable
{
    public class InventoryItemBlackScript : InventoryItemScript
    {
        private const string PrefabPath = "Prefabs/InventoryItemBlack";

        public static void Instantiate(string title, Transform parent, UnityAction onClick)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var objectInstance = GameObject.Instantiate(prefab, parent).GetComponent<InventoryItemScript>();

            objectInstance.ItemInstanceId = title;
            objectInstance.Title.text = title;
            objectInstance.Button.onClick.AddListener(onClick);
        }
    }
}