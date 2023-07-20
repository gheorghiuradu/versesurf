using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Assets.Scripts.Reusable
{
    public class ButtonWithIcon : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IDeselectHandler
    {
        public Color32 NormalColor;
        public Color32 SelectedColor;

        public void OnDeselect(BaseEventData eventData)
        {
            this.SetColors(NormalColor);
            this.GetComponent<Button>().OnDeselect(eventData);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.SetColors(SelectedColor);
            this.GetComponent<Button>().OnPointerEnter(eventData);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (EventSystem.current.currentSelectedGameObject != this.gameObject)
            {
                this.SetColors(NormalColor);
            }
            this.GetComponent<Button>().OnPointerExit(eventData);
        }

        public void OnSelect(BaseEventData eventData)
        {
            this.SetColors(SelectedColor);
            this.GetComponent<Button>().OnSelect(eventData);
        }

        public void Select()
        {
            this.GetComponent<Button>().Select();
        }

        private void SetColors(Color32 color)
        {
            this.GetComponent<NicerOutline>().effectColor = color;
            foreach (var tmp in this.GetComponentsInChildren<TextMeshProUGUI>())
            {
                tmp.color = color;
            }
            foreach (var image in this.GetComponentsInChildren<Image>())
            {
                if (image.sprite != null)
                    image.color = color;
            }
            foreach (var outline in this.GetComponentsInChildren<NicerOutline>())
            {
                outline.effectColor = color;
            }
        }
    }
}