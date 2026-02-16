using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Services;
using SharedDomain.Messages.Commands;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.VIP
{
    public class VipManager : MonoBehaviourEventProvider
    {
        private MusicClient musicClient;
        private AudioSource audioSource;

        private bool hasAttemptedAutoActivation;
        private bool hasNotifiedExpiration;

        public bool IsVip => VipPerks?.Count > 0;
        public HashSet<VipPerk> VipPerks { get; } = new HashSet<VipPerk>();
        public UnityEvent VipActivated { get; } = new UnityEvent();

        private void Start()
        {
            GameObject.DontDestroyOnLoad(this.gameObject);
            this.audioSource = this.GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            this.musicClient.GameEnded.AddListener(this.OnGameEnded);
        }

        private async void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            await new WaitForEndOfFrame();
            while (GameObject.FindObjectOfType<Fader>() != null)
            {
                //Wait for fader scene transition
                await new WaitForSeconds(0.5f);
            }

            if (!this.IsVip)
            {
                await this.AttemptAutoActivation();
            }
            else
            {
                this.ApplyVipAssets();
            }
        }

        private void OnGameEnded()
        {
        }

        public async Task AttemptAutoActivation()
        {
            if (!this.hasAttemptedAutoActivation && !this.IsVip)
            {
                this.ApplyVipAssets();
                this.audioSource.PlayOneShot(Constants.AudioClips.GetVipActivatedSound());
                this.hasNotifiedExpiration = false;
            }
        }

        public void ApplyVipAssets()
        {
            foreach (var asset in GameObject.FindObjectOfType<Canvas>().GetComponentsInChildren<IVipAsset>(true))
            {
                asset.ApplyVip();
            }
        }

        public void DiscardVipAssets()
        {
            foreach (var asset in GameObject.FindObjectOfType<Canvas>().GetComponentsInChildren<IVipAsset>(true))
            {
                asset.DiscardVip();
            }
        }

        public static void Initialize(MusicClient musicClient)
        {
            var instance = new GameObject(nameof(VipManager)).AddComponent<VipManager>();
            instance.gameObject.AddComponent<AudioSource>();
            instance.musicClient = musicClient;
        }
    }
}