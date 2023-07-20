using Assets.Scripts.Services;
using SharedDomain;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class PlayerButtonScript : MonoBehaviour
    {
        public Image Character;
        public TextMeshProUGUI PlayerName;

        [HideInInspector]
        public Player Player { get; private set; }

        public const string PrefabPath = "Prefabs/PlayerButtonWhite";

        public static PlayerButtonScript Instantiate(Player player, Transform parent, UnityAction onClick)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent).GetComponent<PlayerButtonScript>();
            instance.Character.sprite = Resources.Load<Sprite>($"{Constants.CharacterPath}/{player.CharacterCode}/ArtBoard 4");
            instance.Character.preserveAspect = true;
            instance.PlayerName.text = player.Nick;
            instance.Player = player;
            instance.GetComponent<Button>().onClick.AddListener(onClick);

            return instance;
        }
    }
}