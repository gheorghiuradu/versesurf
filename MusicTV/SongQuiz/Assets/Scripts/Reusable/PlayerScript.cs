using Assets.Scripts.Services;
using SharedDomain;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Reusable
{
    public class PlayerScript : MonoBehaviour, IEquatable<PlayerScript>
    {
        private const string PlayerPrefabPath = "Prefabs/Player";

        [SerializeField]
        private TextMeshProUGUI PlayerName;

        [SerializeField]
        private Image Character;

        [SerializeField]
        private Animator CharacterAnimator;

        private Player player;
        private AudioSource audioSource;
        private AudioPeer audioPeer;
        private bool isDancing;

        public bool PlayerLoaded { get; private set; }
        public ActionState ActionState { get; private set; }
        public string Id => this.player is null ? string.Empty : this.player.Id;
        public string PlayerNick => this.player is null ? string.Empty : this.player.Nick;
        public string CharacterCode { get; private set; }

        private void Start()
        {
            if (this.player is null)
            {
                // Bug cannot change sprite while animator is enabled see https://answers.unity.com/questions/1013120/cant-change-sprite-when-animator-component-is-atta.html
                this.CharacterAnimator.enabled = false;
                this.Character.sprite = Resources.Load<Sprite>($"{Constants.CharacterPath}/{this.CharacterCode}/ArtBoard 1");
                this.Character.preserveAspect = true;
            }
        }

        private void LateUpdate()
        {
            this.PlayerName.rectTransform.sizeDelta = new Vector2(this.GetComponent<RectTransform>().sizeDelta.x,
                                                    this.PlayerName.rectTransform.sizeDelta.y);

            // Set if dancing
            if (this.audioSource != null && this.CharacterAnimator != null)
            {
                if (this.audioSource.isPlaying &&
                    !this.isDancing)
                {
                    this.isDancing = true;
                    this.PlayClipAsync(Constants.Animations.CharacterDance + this.CharacterCode).CatchErrors();
                }
                else if (!this.audioSource.isPlaying &&
                    this.isDancing)
                {
                    this.isDancing = false;
                    this.CharacterAnimator.speed = 0;
                }
            }

            // Set dance speed
            if (this.audioPeer != null && this.isDancing)
            {
                // Get bands from base to mid
                var targetSpeed = (this.audioPeer.AudioBands[0] + this.audioPeer.AudioBands[1]
                                    + this.audioPeer.AudioBands[3]) / 3
                                    * 3f;
                float speedStep;
                if (targetSpeed == 0)
                {
                    speedStep = 1;
                }
                else
                {
                    speedStep = targetSpeed > this.CharacterAnimator.speed ?
                        targetSpeed / this.CharacterAnimator.speed : this.CharacterAnimator.speed / targetSpeed;
                }
                this.CharacterAnimator.speed =
                    Mathf.Lerp(this.CharacterAnimator.speed, targetSpeed, speedStep);
            }
        }

        public void ListenTo(AudioSource audioSource, AudioPeer audioPeer)
        {
            this.audioSource = audioSource;
            this.audioPeer = audioPeer;
        }

        public void LoadPlayer(Player player, ActionState state = ActionState.Completed)
        {
            if (this.PlayerLoaded) throw new System.Exception("Player already loaded");

            this.player = player;

            this.CharacterCode = player.CharacterCode;
            // Bug cannot change sprite while animator is enabled
            this.CharacterAnimator.enabled = false;
            this.Character.sprite = Resources.Load<Sprite>($"{Constants.CharacterPath}/{this.CharacterCode}/ArtBoard 4");
            this.Character.preserveAspect = true;
            this.PlayerName.text = player.Nick;

            this.SetActionState(state);

            this.PlayerLoaded = true;
        }

        public void ReloadPlayer(Player player)
        {
            if (!this.PlayerLoaded) throw new System.Exception("Player is not loaded");

            this.player = player;
            this.PlayerName.text = player.Nick;
        }

        public Task AnimateJoinAsync()
        {
            return this.PlayClipAsync(Constants.Animations.CharacterJoin + this.CharacterCode);
        }

        public void SetActionState(ActionState state)
        {
            this.ActionState = state;
            switch (state)
            {
                case ActionState.Completed:
                    this.Character.color = Color.white;
                    break;

                case ActionState.Pending:
                    this.Character.color = new Color(1, 1, 1, 0.5f);
                    break;
            }
        }

        public static PlayerScript Instantiate(Transform parent, string characterCode)
        {
            var prefab = Resources.Load<GameObject>(PlayerPrefabPath);
            var instance = GameObject.Instantiate(prefab, parent).GetComponent<PlayerScript>();
            instance.CharacterCode = characterCode;

            return instance;
        }

        public static PlayerScript Instantiate(Transform parent, Player player, ActionState state = ActionState.Completed)
        {
            var instance = Instantiate(parent, player.CharacterCode);
            instance.LoadPlayer(player, state);

            return instance;
        }

        private async Task PlayClipAsync(string clipName)
        {
            if (!this.CharacterAnimator.enabled)
            {
                this.CharacterAnimator.enabled = true;
            }
            if (this.CharacterAnimator.speed == 0)
            {
                this.CharacterAnimator.speed = 1;
            }

            var clip = System.Array.Find(this.CharacterAnimator.runtimeAnimatorController.animationClips,
                c => c.name.Equals(clipName));

            this.CharacterAnimator.Play(clipName);
            await new WaitForSeconds(clip.length);
        }

        public bool Equals(PlayerScript other)
        {
            return this.Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }

    public enum ActionState
    {
        Completed,
        Pending
    }
}