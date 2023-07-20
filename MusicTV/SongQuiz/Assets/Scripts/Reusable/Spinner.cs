using UnityEngine;

namespace Assets.Scripts.Reusable
{
    public class Spinner : MonoBehaviour
    {
        public bool IsSpinning = false;
        public float RotateSpeed = -200f;

        private RectTransform rectComponent;

        private void Start()
        {
            rectComponent = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (this.IsSpinning)
                rectComponent.Rotate(0f, 0f, RotateSpeed * Time.deltaTime);
        }
    }
}