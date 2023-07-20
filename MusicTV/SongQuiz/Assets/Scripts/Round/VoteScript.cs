using Assets.Scripts.Services;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Round
{
    public class VoteScript : MonoBehaviour
    {
        public TextMeshProUGUI PlayerName;
        public TextMeshProUGUI Points;

        public async Task ShowAsync()
        {
            this.gameObject.SetActive(true);
            var animator = this.GetComponent<Animator>();
            var clip = animator.runtimeAnimatorController.animationClips[0];

            animator.Play(clip.name);
            await new WaitForSeconds(clip.length + 0.1f);
        }

        /// <summary>
        /// Shows points and sets green.
        /// Call this when correct answer
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public async Task ShowCorrectAsync(int points)
        {
            this.GetComponent<Image>().color = Constants.Colors.CorrectAnswerBackground;
            this.PlayerName.color = Constants.Colors.CorrectAnswerText;
            this.Points.text = $"+{points}";
            this.Points.gameObject.SetActive(true);
            await new WaitForSeconds(2);
        }
    }
}