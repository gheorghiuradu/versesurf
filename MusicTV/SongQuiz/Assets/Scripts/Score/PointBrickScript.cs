using Assets.Scripts.Services;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Assets.Scripts.Score
{
    public class PointBrickScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/PointBrick";

        private Animator animator;
        private Image image;
        private NicerOutline nicerOutline;

        public bool FinishedFadeIn { get; private set; }

        private void Start()
        {
            this.animator = this.GetComponent<Animator>();
            this.image = this.GetComponent<Image>();
            this.nicerOutline = this.GetComponent<NicerOutline>();
        }

        private void SetColor(Color32 color)
        {
            if (this.image is null)
            {
                this.image = this.GetComponent<Image>();
            }
            this.image.color = color;
        }

        private async Task FadeInAnimateAsync()
        {
            this.FinishedFadeIn = false;
            if (this.animator is null)
            {
                this.animator = this.GetComponent<Animator>();
            }
            var clip = this.animator.runtimeAnimatorController.animationClips[0];

            this.animator.Play(clip.name);
            await new WaitForSeconds(clip.length);

            this.FinishedFadeIn = true;
        }

        public static GameObject InstantiateInitial(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent);
            instance.transform.SetAsFirstSibling();
            var instanceScript = instance.GetComponent<PointBrickScript>();
            instanceScript.FadeInAnimateAsync().CatchErrors();
            instanceScript.SetColor(Constants.Colors.InitialScoreBrick);

            return instance;
        }

        public static async Task<GameObject> InstantiateAndFadeInAsync(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent);
            instance.transform.SetAsFirstSibling();

            var instanceScript = instance.GetComponent<PointBrickScript>();
            instanceScript.SetColor(Constants.Colors.NewScoreBrick);
            await instanceScript.FadeInAnimateAsync();

            return instance;
        }
    }
}