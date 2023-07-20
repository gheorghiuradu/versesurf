using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class AudioSourceExtensions
    {
        public static async Task FadeOut(this AudioSource audioSource)
        {
            while (audioSource.isPlaying && audioSource.volume > 0f)
            {
                await new WaitForSeconds(1);
                if (audioSource.volume < 0.1f || audioSource.volume.IsEqualTo(0.1f))
                {
                    audioSource.volume -= 0.025f;
                }
                else
                {
                    audioSource.volume -= 0.1f;
                }
            }
        }
    }
}