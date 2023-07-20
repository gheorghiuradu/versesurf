using TMPro;
using UnityEngine;

namespace Assets.Scripts.News
{
    public class NewsItemScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/NewsItem";
        private Vector2 initialSize;

        public TextMeshProUGUI Title;
        public TextMeshProUGUI Date;
        public TextMeshProUGUI Body;

        private void Start()
        {
            this.initialSize = this.GetComponent<RectTransform>().sizeDelta;
        }

        private void OnGUI()
        {
            var rectTransform = this.GetComponent<RectTransform>();
            var requiredHeight = this.Body.GetRenderedValues().y + this.initialSize.y;
            if (requiredHeight != rectTransform.sizeDelta.y)
            {
                rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);
                this.Body.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, this.Body.GetRenderedValues().y);
            }
        }

        public static NewsItemScript Instantiate(string title, string date, string body, Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent).GetComponent<NewsItemScript>();
            instance.Title.text = title;
            instance.Date.text = date;
            instance.Body.text = body;

            return instance;
        }
    }
}