using UnityEngine;

namespace Assets.Scripts.VIP
{
    public class VipLogo : VipImage
    {
        private const string VipLogoPath = "Sprites/VipLogo";

        internal override void LoadVipAsset()
        {
            this.VipAsset = Resources.Load<Sprite>(VipLogoPath);
        }
    }
}