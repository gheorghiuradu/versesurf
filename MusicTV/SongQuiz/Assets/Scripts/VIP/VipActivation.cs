using UnityEngine;

namespace Assets.Scripts.VIP
{
    public class VipActivation : MonoBehaviour, IVipAsset
    {
        public void ApplyVip() => this.gameObject.SetActive(true);

        public void DiscardVip() => this.gameObject.SetActive(false);
    }
}