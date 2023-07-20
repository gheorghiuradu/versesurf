using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Panels
{
    public class AlertPanelScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/AlertPanel";

        public TextMeshProUGUI TitleTMP;
        public TextMeshProUGUI MessageTMP;
        public Button OKButton;

        private void Start()
        {
            this.OKButton.Select();
            this.OKButton.OnSelect(new BaseEventData(EventSystem.current));
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                GameObject.Destroy(this.gameObject);
            }
        }

        public static void Instantiate(string title, string message)
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, canvas.transform);

            instance.GetComponent<AlertPanelScript>().MessageTMP.text = message;
            instance.GetComponent<AlertPanelScript>().TitleTMP.text = title;
            instance.transform.SetAsLastSibling();
        }

        public void OnOKPress()
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}