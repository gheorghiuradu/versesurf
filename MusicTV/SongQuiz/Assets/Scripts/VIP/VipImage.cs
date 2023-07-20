using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.VIP
{
    [RequireComponent(typeof(Image))]
    public class VipImage : MonoBehaviour, IVipAsset
    {
        private Sprite originalAsset;
        public Sprite VipAsset { get; internal set; }

        public void ApplyVip()
        {
            if (this.VipAsset == null)
            {
                this.LoadVipAsset();
            }
            if (this.VipAsset != null)
            {
                var image = this.GetComponent<Image>();
                this.originalAsset = image.sprite;
                image.sprite = this.VipAsset;
            }
        }

        public void DiscardVip() => this.GetComponent<Image>().sprite = this.originalAsset;

        internal virtual void LoadVipAsset()
        {
        }
    }
}