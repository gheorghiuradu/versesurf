using SharedDomain;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Score
{
    public class ScoreItemScript : MonoBehaviour
    {
        public TextMeshProUGUI PlayerNameTMP;
        public TextMeshProUGUI PointsTMP;
        public Animator CoverAnimator;
        public Image StarPanelImage;

        public async Task ShowScoreAsync(Player player, int score)
        {
            this.PlayerNameTMP.text = player.Nick;
            this.PointsTMP.text = string.Empty;
            ColorUtility.TryParseHtmlString(player.ColorCode, out var playerColor);
            this.StarPanelImage.color = playerColor;

            this.PointsTMP.text = score.ToString();
            var clip = this.CoverAnimator.runtimeAnimatorController.animationClips[0];
            this.CoverAnimator.Play(clip.name);
            this.GetComponent<AudioSource>().Play();

            await new WaitForSeconds(clip.length);
        }

        private void Awake()
        {
            this.CoverAnimator.GetComponent<RectTransform>().sizeDelta = this.GetComponent<RectTransform>().sizeDelta;
        }
    }
}