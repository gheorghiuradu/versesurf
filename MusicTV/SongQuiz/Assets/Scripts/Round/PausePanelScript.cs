using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Round
{
    public class PausePanelScript : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/PausePanel";
        private Room room;
        private UnityAction<string> onPlayerKicked;
        private GameOptions gameOptions;
        private bool changedVolume;

        public Button ResumeButton;
        public IncrementalControlScript VolumeControl;
        public Button RestartButton;
        public Button KickPlayerButton;
        public AudioSource AudioSource;

        public bool ShouldRestart { get; set; }

        public System.Action OnResume { get; set; }
        public System.Action OnExit { get; set; }
        public System.Func<Task> OnExitAsync { get; set; }

        private void Start()
        {
            this.SelectFirstEnabledButton();
            this.room = ServiceProvider.Get<Room>();
            this.gameOptions = ServiceProvider.Get<GameOptions>();

            this.VolumeControl.Value = this.gameOptions.Volume;
            this.VolumeControl.MinValue = GameOptions.MinVolume;
            this.VolumeControl.MaxValue = GameOptions.MaxVolume;
            this.gameOptions.ApplyVolume(this.gameObject);
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null &&
                this.transform.IsLastChild())
            {
                this.SelectFirstEnabledButton();
            }

            if (this.room?.Players.Count > 2 && !this.KickPlayerButton.interactable)
            {
                this.KickPlayerButton.interactable = true;
            }
            else if (this.room?.Players.Count <= 2 && this.KickPlayerButton.interactable)
            {
                this.KickPlayerButton.interactable = false;
            }

            if (!this.transform.IsLastChild() && this.KickPlayerButton.interactable)
            {
                this.DisableAllButtons();
            }
        }

        private void OnDestroy()
        {
            if (this.changedVolume)
            {
                ServiceProvider.Get<PlayFabService>().SaveGameOptionsAsync(this.gameOptions).CatchErrors();
                ServiceProvider.AddOrReplace(this.gameOptions);
            }
        }

        public async void Exit()
        {
            if (!(this.OnExit is null))
            {
                this.OnExit();
            }
            if (!(this.OnExitAsync is null))
            {
                await this.OnExitAsync();
            }
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }

        public void Resume()
        {
            if (this.ShouldRestart)
            {
                this.Restart();
            }
            else
            {
                GameObject.Destroy(this.gameObject);
                if (!(this.OnResume is null))
                    this.OnResume();
            }
        }

        public void Restart()
        {
            foreach (var button in this.GetComponentsInChildren<Button>())
            {
                button.interactable = false;
            }
            LoadingSpinner.Instantiate();
            Time.timeScale = 1;
            this.room.CurrentRound.Score = new ScoreBoard(this.room.Players);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void OpenKickPlayerPanel()
        {
            KickPlayerPanel.Instantiate(this.transform.parent, () =>
            {
                this.EnableAllButtons();
                this.SelectFirstEnabledButton();
            }, new UnityAction<string>[] { this.onPlayerKicked, _ => this.ResumeButton.gameObject.SetActive(false) });
            this.DisableAllButtons();
        }

        public void SetVolume()
        {
            this.gameOptions.Volume = this.VolumeControl.Value;
            this.gameOptions.ApplyVolume();
            if (this.AudioSource.isPlaying)
            {
                this.AudioSource.Stop();
            }
            this.AudioSource.PlayOneShot(Constants.AudioClips.GetPointsTicker01Sound());
            this.changedVolume = true;
        }

        public static PausePanelScript Instantiate(
            System.Action onResume = null,
            System.Action onExit = null,
            UnityAction<string> onPlayerKicked = null)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var instanceScript = GameObject.Instantiate(prefab, canvas.transform).GetComponent<PausePanelScript>();
            instanceScript.OnResume = onResume;
            instanceScript.OnExit = onExit;
            instanceScript.onPlayerKicked = onPlayerKicked;

            return instanceScript;
        }
    }
}