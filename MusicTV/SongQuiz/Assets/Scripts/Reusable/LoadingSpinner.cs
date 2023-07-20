using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.Reusable
{
    public class LoadingSpinner : MonoBehaviour
    {
        public GameObject TextPanel;
        public TextMeshProUGUI TextMessage;

        private const string PrefabPath = "Prefabs/LoadingSpinner";

        public static void Instantiate(string message)
        {
            Instantiate(message, GetDefaultParent());
        }

        public static void Instantiate(string message, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);

            var instance = GameObject.Instantiate(prefab, parent.transform).GetComponent<LoadingSpinner>();
            instance.transform.SetAsLastSibling();
            instance.TextMessage.text = message;
            instance.TextPanel.SetActive(true);
        }

        public static void Instantiate(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);

            var instance = GameObject.Instantiate(prefab, parent);
            instance.transform.SetAsLastSibling();
        }

        public static void Instantiate()
        {
            Instantiate(GetDefaultParent());
        }

        public static void Destroy()
        {
            var instance = GameObject.FindObjectOfType<LoadingSpinner>();
            GameObject.Destroy(instance.gameObject);
        }

        private static Transform GetDefaultParent()
        {
            return SceneManager.GetActiveScene().GetRootGameObjects()
                .FirstOrDefault(g => g.GetComponent<Canvas>() != null).transform;
        }
    }
}