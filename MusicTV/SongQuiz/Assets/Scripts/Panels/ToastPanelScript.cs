using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain.Messages.Commands;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Panels
{
    public class ToastPanelScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/ToastPanel";
        private const string FadeOutAnimName = "ToastFadeOut";

        public float SecondsToShow;
        public TextMeshProUGUI TitleTMP;
        public TextMeshProUGUI TextTMP;

        private void Start()
        {
            var options = ServiceProvider.Get<GameOptions>();
            if (!(options is null))
            {
                options.ApplyVolume(this.gameObject);
            }
        }

        public async void Show()
        {
            this.GetComponent<AudioSource>().Play();
            await new WaitForSeconds(SecondsToShow);

            var animator = this.GetComponent<Animator>();
            var clip = animator.runtimeAnimatorController.animationClips.FirstOrDefault(c => c.name.Equals(FadeOutAnimName));
            animator.Play(FadeOutAnimName);
            await new WaitForSeconds(clip.length);

            GameObject.Destroy(this.gameObject);
        }

        public static void Instantiate(NotificationMessage notification, float secondsToShow = 5)
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, canvas.transform).GetComponent<ToastPanelScript>();

            instance.SecondsToShow = secondsToShow;
            instance.TextTMP.text = notification.Message;
            instance.TitleTMP.text = (notification.Title ?? "NOTIFICATION").ToUpper();
            instance.transform.SetAsLastSibling();
            instance.Show();
        }
    }
}