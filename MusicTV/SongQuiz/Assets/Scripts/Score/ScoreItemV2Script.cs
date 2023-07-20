using Assets.Scripts.Reusable;
using SharedDomain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Score
{
    public class ScoreItemV2Script : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/ScoreItemV2";

        public TextMeshProUGUI PointsTMPro;
        public PlayerScript Player;

        public bool FinishedShowingScore { get; private set; }

        private void SetScoreText(int score)
        {
            this.PointsTMPro.text = score.ToString();
            this.PointsTMPro.transform.SetAsFirstSibling();
        }

        public static async Task<GameObject> InstantiateAndShowScoreAsync(
            Player player,
            KeyValuePair<int, int> initialScore,
            KeyValuePair<int, int> addedScore,
            Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent);
            var instanceScript = instance.GetComponent<ScoreItemV2Script>();

            instanceScript.Player.LoadPlayer(player);
            for (int i = 0; i < initialScore.Key; i++)
            {
                PointBrickScript.InstantiateInitial(instance.transform);
                instanceScript.PointsTMPro.transform.SetAsFirstSibling();
            }

            for (int i = 0; i < addedScore.Key; i++)
            {
                await new WaitForSeconds(0.2f);
                await PointBrickScript.InstantiateAndFadeInAsync(instance.transform);
                instanceScript.PointsTMPro.transform.SetAsFirstSibling();
            }

            while (instance.GetComponentsInChildren<PointBrickScript>().Any(pbs => !pbs.FinishedFadeIn))
            {
                //wait
                await new WaitForSeconds(0.2f);
            }

            instanceScript.SetScoreText(initialScore.Value + addedScore.Value);
            instanceScript.FinishedShowingScore = true;

            return instance;
        }
    }
}