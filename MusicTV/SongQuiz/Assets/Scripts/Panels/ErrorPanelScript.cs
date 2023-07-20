using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Panels
{
    public class ErrorPanelScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/ErrorPanel";

        public TextMeshProUGUI MessageTMP;
        public Button OKButton;
        public UnityEvent OnOkPressed { get; } = new UnityEvent();

        private void Start()
        {
            this.OKButton.Select();
            this.OKButton.OnSelect(new BaseEventData(EventSystem.current));
        }

        private void LateUpdate()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.OnOKPress();
            }
        }

        public static ErrorPanelScript Instantiate(string errorMessage)
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, canvas.transform);

            instance.GetComponent<ErrorPanelScript>().MessageTMP.text = errorMessage;
            instance.transform.SetAsLastSibling();

            return instance.GetComponent<ErrorPanelScript>();
        }

        public void OnOKPress()
        {
            this.OnOkPressed.Invoke();
            GameObject.Destroy(this.gameObject);
        }
    }
}