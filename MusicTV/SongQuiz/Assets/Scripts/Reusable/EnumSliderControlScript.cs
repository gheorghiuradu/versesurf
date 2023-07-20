using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class EnumSliderControlScript : Selectable, IIncrementalControlScript
    {
        private List<string> values;
        private int currentIndex;

        public string Value { get; private set; }
        public TextMeshProUGUI Counter;
        public IncrementerButtonScript IncreaseButton;
        public IncrementerButtonScript DecreaseButton;
        public Image CounterContainer;

        [SerializeField]
        public StringEvent OnValueChanged;

        public bool IsFocused { get; private set; }

        public void Load(string value, System.Type enumType)
        {
            this.values = System.Enum.GetNames(enumType).ToList();
            this.Value = value;
            this.currentIndex = this.values.IndexOf(value);
        }

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
            this.Counter.text = this.Value;
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
            if (this.currentIndex >= this.values.Count - 1)
            {
                this.currentIndex = -1;
            }
            this.currentIndex++;
            this.Value = this.values[this.currentIndex];
            this.OnValueChanged?.Invoke(this.Value);
        }

        public void Decrease()
        {
            if (this.currentIndex == 0)
            {
                this.currentIndex = this.values.Count;
            }
            this.currentIndex--;
            this.Value = this.values[this.currentIndex];
            this.OnValueChanged?.Invoke(this.Value);
        }

        [System.Serializable]
        public class StringEvent : UnityEvent<string> { };
    }
}