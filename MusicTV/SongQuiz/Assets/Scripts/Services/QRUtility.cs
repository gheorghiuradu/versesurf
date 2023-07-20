using QRCoder;
using QRCoder.Unity;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public static class QRUtility
    {
        public static Texture2D GenerateQrCodeTexture(string data, int pixelsPerModule = 20)
        {
            var qrData = new QRCodeGenerator().CreateQrCode(data,
                    QRCodeGenerator.ECCLevel.Q);
            return new UnityQRCode(qrData).GetGraphic(pixelsPerModule);
        }
    }
}