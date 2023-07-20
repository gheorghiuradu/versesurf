using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class IncrementerButtonScript : MonoBehaviour, IPointerDownHandler
    {
        public Color NormalColor;
        public Color PressedColor;
        public InControlType ControlType;

        private IIncrementalControlScript parentControl;
        private Image image;

        private void Start()
        {
            this.parentControl = this.GetComponentInParent<IIncrementalControlScript>();
            this.image = this.GetComponent<Image>();
        }

        public async void Click()
        {
            this.image.color = this.PressedColor;
            switch (this.ControlType)
            {
                case InControlType.Increment:
                    if (!(this.parentControl is null))
                        this.parentControl.Increment();
                    break;

                case InControlType.Decrease:
                    if (!(this.parentControl is null))
                        this.parentControl.Decrease();
                    break;
            }
            await new WaitForSecondsRealtime(0.2f);
            this.image.color = this.NormalColor;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            this.parentControl.Select();
            this.Click();
        }

        public enum InControlType
        {
            Increment,
            Decrease
        }
    }
}