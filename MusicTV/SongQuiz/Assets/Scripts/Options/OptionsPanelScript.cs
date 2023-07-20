using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.Options
{
    public class OptionsPanelScript : MonoBehaviour, IClosablePanel
    {
        public IncrementalControlScript NumberOfRoundsControl;
        public IncrementalControlScript VolumeControl;
        public Toggle MenuMusicToggle;
        public Toggle AllowExplicitToggle;
        public Toggle FullScreenToggle;
        public ArraySliderControlScript ResolutionControl;

        private PlayFabService playFab;
        private GameOptions gameOptions;
        private AudioSource audioSource;

        private void Start()
        {
            this.playFab = ServiceProvider.Get<PlayFabService>();
            this.gameOptions = ServiceProvider.Get<GameOptions>();
            this.audioSource = this.GetComponent<AudioSource>();

            this.NumberOfRoundsControl.Value = this.gameOptions.NumberOfRounds;
            this.NumberOfRoundsControl.MaxValue = GameOptions.MaxNumberOfRounds;
            this.NumberOfRoundsControl.MinValue = GameOptions.MinNumberOfRounds;

            this.VolumeControl.Value = this.gameOptions.Volume;
            this.VolumeControl.MinValue = GameOptions.MinVolume;
            this.VolumeControl.MaxValue = GameOptions.MaxVolume;

            this.MenuMusicToggle.isOn = this.gameOptions.MenuMusic;
            this.AllowExplicitToggle.isOn = this.gameOptions.AllowExplicit;

#if UNITY_STANDALONE
            this.FullScreenToggle.isOn = Screen.fullScreen;
            this.FullScreenToggle.onValueChanged.AddListener(fs => Screen.fullScreen = fs);
            this.ResolutionControl.Load(Screen.currentResolution.ToString().Split(new string[] { " @ " }, StringSplitOptions.None)[0],
                Screen.resolutions
                .Where(r => r.GetAspectRatio() == Screen.currentResolution.GetAspectRatio())
                .Select(r => r.ToString().Split(new string[] { " @ " }, StringSplitOptions.None)[0])
                .ToList());
            this.ResolutionControl.OnValueChanged.AddListener(this.SetResolution);
#else
            this.FullScreenToggle.gameObject.SetActive(false);
            this.ResolutionControl.gameObject.SetActive(false);
#endif

            this.NumberOfRoundsControl.Select();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && this.transform.IsLastChild())
            {
                this.UpdateAndCloseAsync();
            }
        }

        public async void UpdateAndCloseAsync()
        {
            this.gameOptions.NumberOfRounds = this.NumberOfRoundsControl.Value;
            this.gameOptions.AllowExplicit = this.AllowExplicitToggle.isOn;
            this.gameOptions.MenuMusic = this.MenuMusicToggle.isOn;
            this.gameOptions.Volume = this.VolumeControl.Value;

#if UNITY_STANDALONE
            this.gameOptions.FullScreen = this.FullScreenToggle.isOn;
            this.gameOptions.SetResolution(this.GetSelectedResolution());
#endif

            await this.playFab.SaveGameOptionsAsync(this.gameOptions);

            ServiceProvider.AddOrReplace(this.gameOptions);

            GameObject.Destroy(this.gameObject);
        }

        public void Close()
        {
            this.UpdateAndCloseAsync();
        }

        public void OpenGuide()
        {
            TutorialPanelScript.Instantiate();
        }

        public static OptionsPanelScript Instantiate()
        {
            PanelManager.CloseAllOpenPanels();
            var prefab = Resources.Load<GameObject>(Constants.OptionsPanelPrefabPath);
            var canvas = GameObject.FindObjectOfType<Canvas>();

            return GameObject.Instantiate(prefab, canvas.transform).GetComponent<OptionsPanelScript>();
        }

        public void SetVolume()
        {
            this.gameOptions.Volume = this.VolumeControl.Value;
            this.gameOptions.ApplyVolume();
            if (this.audioSource.isPlaying)
            {
                this.audioSource.Stop();
            }
            this.audioSource.PlayOneShot(Constants.AudioClips.GetPointsTicker01Sound());
        }

        private UnityEngine.Resolution GetSelectedResolution()
        {
            return this.ResolutionControl.Value.ToResolution();
        }

        private void SetResolution(string selectedResolution)
        {
            var resolution = selectedResolution.ToResolution();
            Screen.SetResolution(resolution.width, resolution.height, this.FullScreenToggle.isOn);
        }
    }
}