using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class IncrementalControlScript : Selectable, IIncrementalControlScript
    {
        public int Value;
        public int MaxValue;
        public int MinValue;
        public int Step = 1;
        public TextMeshProUGUI Counter;
        public IncrementerButtonScript IncreaseButton;
        public IncrementerButtonScript DecreaseButton;
        public Image CounterContainer;

        [SerializeField]
        public IntEvent OnValueChanged;

        public bool IsFocused { get; private set; }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            this.SetSelectedUI();
            base.OnPointerEnter(eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!this.IsFocused)
            {
                this.SetDeselectedUI();
            }
            base.OnPointerExit(eventData);
        }

        public override void Select()
        {
            this.IsFocused = true;
            base.Select();
        }

        private void SetSelectedUI()
        {
            this.CounterContainer.enabled = true;
        }

        private void SetDeselectedUI()
        {
            this.CounterContainer.enabled = false;
        }

        public override void OnSelect(BaseEventData eventData)
        {
            this.IsFocused = true;
            base.OnSelect(eventData);
            this.SetSelectedUI();
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            this.IsFocused = false;
            base.OnDeselect(eventData);
            this.SetDeselectedUI();
        }

        private void Update()
        {
            this.Counter.text = this.Value.ToString();
            if (Input.GetKeyDown(KeyCode.RightArrow) && this.IsFocused)
            {
                this.IncreaseButton.Click();
            }
            if (Input.GetKeyDown(KeyCode.LeftArrow) && this.IsFocused)
            {
                this.DecreaseButton.Click();
            }
        }

        public void Increment()
        {
            if (this.Value < this.MaxValue)
            {
                this.Value += this.Step;
                this.OnValueChanged?.Invoke(this.Value);
            }
        }

        public void Decrease()
        {
            if (this.Value > this.MinValue)
            {
                this.Value -= this.Step;
                this.OnValueChanged?.Invoke(this.Value);
            }
        }

        [System.Serializable]
        public class IntEvent : UnityEvent<int> { };
    }
}