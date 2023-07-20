using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class CameraExtensions
    {
        public static Vector2 GetScreenCenter(this Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector2(Screen.width / 2f, Screen.height / 2f));
        }

        public static Vector2 GetScreenTopCenter(this Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector2(Screen.width / 2f, Screen.height));
        }

        public static Vector2 GetScreenLeftMiddle(this Camera camera)
        {
            return camera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height / 2f));
        }
    }
}