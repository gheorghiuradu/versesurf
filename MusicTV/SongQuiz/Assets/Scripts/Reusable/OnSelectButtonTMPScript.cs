using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class OnSelectButtonTMPScript : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
    {
        public Color32 NormalTextColor;
        public Color32 OnSelectedTextColor;

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.SetTextChildrenColor(this.OnSelectedTextColor);
            this.GetComponent<Button>().OnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject != this.gameObject)
            {
                this.SetTextChildrenColor(this.NormalTextColor);
            }
            this.GetComponent<Button>().OnPointerExit(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            this.SetTextChildrenColor(this.OnSelectedTextColor);
            this.GetComponent<Button>().OnSelect(eventData);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            this.SetTextChildrenColor(this.NormalTextColor);
            this.GetComponent<Button>().OnDeselect(eventData);
        }

        private void SetTextChildrenColor(Color32 color)
        {
            foreach (var tmPro in this.GetComponentsInChildren<TextMeshProUGUI>())
            {
                tmPro.color = color;
            }
        }
    }
}