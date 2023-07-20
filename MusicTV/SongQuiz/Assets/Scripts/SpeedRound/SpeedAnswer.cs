using Assets.Scripts.Extensions;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.SpeedRound
{
    public class SpeedAnswer : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/SpeedAnswer";

        [SerializeField]
        private Image leftImage;

        [SerializeField]
        private Image rightImage;

        [SerializeField]
        private TextMeshProUGUI timeTMP;

        [SerializeField]
        private TextMeshProUGUI answerTMP;

        [SerializeField]
        private TextMeshProUGUI smallTimeTMP;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AudioSource audioSource;

        public async Task RevealAsync()
        {
            var clip = System.Array.Find(this.animator.runtimeAnimatorController.animationClips, c => c.name == "SpeedAnswerTurn");
            var sound = Resources.Load<AudioClip>($"Audio/whoosh_swish_high_big_0{Random.Range(1, 5)}");
            this.animator.Play(clip.name);
            this.audioSource.PlayOneShot(sound);
            await new WaitForSeconds(clip.length);
        }

        public static async void Instantiate(Transform parent, SharedDomain.SpeedAnswer answer, int seconds, bool isCorrect)
        {
            var children = parent.GetChildren().ToList();
            var placeHolder = children.Find(c => c.name.StartsWith("AnswerPlaceholder"));
            var index = children.IndexOf(placeHolder);

            var prefab = Resources.Load<GameObject>(PrefabPath);
            GameObject.Destroy(placeHolder.gameObject);
            var speedAnswer = GameObject.Instantiate(prefab, parent).GetComponent<SpeedAnswer>();
            speedAnswer.name = answer.Player.Id;
            speedAnswer.transform.SetSiblingIndex(index);
            speedAnswer.LoadPlayer(index, answer.Player);
            speedAnswer.timeTMP.text = $"{seconds}s";
            speedAnswer.answerTMP.text = answer.Name;
            speedAnswer.answerTMP.text += isCorrect ? "<sprite index=0>" : "<sprite index=1>";
            speedAnswer.smallTimeTMP.text = $"{seconds}s";
            if (!isCorrect)
            {
                speedAnswer.answerTMP.faceColor = Constants.Colors.Accent2;
            }

            var clip = System.Array.Find(speedAnswer.animator.runtimeAnimatorController.animationClips,
                c => c.name == "SpeedAnswerShrinkIn");
            speedAnswer.animator.Play(clip.name);
            ServiceProvider.Get<GameOptions>().ApplyVolume(speedAnswer.gameObject);
            speedAnswer.audioSource.PlayOneShot(Constants.AudioClips.GetSpeedAnswerInSound());
            await new WaitForSeconds(clip.length);
        }

        private void LoadPlayer(int index, Player player)
        {
            var image = index % 2 == 0 ? this.leftImage : this.rightImage;
            image.sprite = Resources.Load<Sprite>($"Sprites/Characters/{player.CharacterCode}/white");
            image.preserveAspect = true;
            image.gameObject.SetActive(true);
            image.GetComponentInChildren<TextMeshProUGUI>().text = player.Nick;
        }
    }
}