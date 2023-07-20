using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class Texture2DExtensions
    {
        public static Sprite ToSprite(this Texture2D texture)
        {
            return Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
        }
    }
}