using Assets.Scripts.Extensions;
using Assets.Scripts.Services;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.News
{
    public class NewsPanelScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/NewsPanel";

        public RectTransform Content;
        public Selectable VerticalScrollBar;

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && this.transform.IsLastChild())
            {
                this.Close();
            }

            if (EventSystem.current.currentSelectedGameObject == null && this.transform.IsLastChild())
            {
                this.VerticalScrollBar.Select();
            }
        }

        private void OnGUI()
        {
            var newsItems = this.GetComponentsInChildren<NewsItemScript>();
            var requiredContentHeight = newsItems.Sum(ni => ni.GetComponent<RectTransform>().sizeDelta.y);
            if (this.Content.sizeDelta.y != requiredContentHeight)
            {
                this.Content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredContentHeight);
            }
            this.VerticalScrollBar.Select();
        }

        public void Close()
        {
            GameObject.Destroy(this.gameObject);
        }

        public static async Task AutoCheckAsync()
        {
            var playFab = ServiceProvider.Get<PlayFabService>();
            var titles = await playFab.GetTitleNewsAsync();
            if (titles?.Count > 0)
            {
                var canvas = GameObject.FindObjectOfType<Canvas>();
                var prefab = Resources.Load<GameObject>(PrefabPath);
                var instance = GameObject.Instantiate(prefab, canvas.transform).GetComponent<NewsPanelScript>();
                foreach (var title in titles)
                {
                    var localTime = title.Timestamp.ToLocalTime();
                    NewsItemScript.Instantiate(title.Title, $"{localTime.ToShortDateString()} {localTime.ToShortTimeString()}",
                        title.Body, instance.Content);
                }
            }
        }
    }
}