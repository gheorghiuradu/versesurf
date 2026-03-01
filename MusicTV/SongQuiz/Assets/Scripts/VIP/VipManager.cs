using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Scripts.Reusable;
using Assets.Scripts.Services;
using SharedDomain.Messages.Commands;
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

        public HashSet<VipPerk> VipPerks { get; } = new() { VipPerk.SelectPlaylist };

        public UnityEvent VipActivated { get; } = new();

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            SceneManager.sceneLoaded += OnSceneLoaded;
            musicClient.GameEnded.AddListener(OnGameEnded);
        }

        private async void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            await new WaitForEndOfFrame();
            while (FindObjectOfType<Fader>() != null)
                //Wait for fader scene transition
                await new WaitForSeconds(0.5f);

            if (!IsVip)
                await AttemptAutoActivation();
            else
                ApplyVipAssets();
        }

        private void OnGameEnded()
        {
        }

        public async Task AttemptAutoActivation()
        {
            if (!hasAttemptedAutoActivation && !IsVip)
            {
                ApplyVipAssets();
                audioSource.PlayOneShot(Constants.AudioClips.GetVipActivatedSound());
                hasNotifiedExpiration = false;
            }
        }

        public void ApplyVipAssets()
        {
            foreach (var asset in FindObjectOfType<Canvas>().GetComponentsInChildren<IVipAsset>(true)) asset.ApplyVip();
        }

        public void DiscardVipAssets()
        {
            foreach (var asset in FindObjectOfType<Canvas>().GetComponentsInChildren<IVipAsset>(true)) asset.DiscardVip();
        }

        public static void Initialize(MusicClient musicClient)
        {
            var instance = new GameObject(nameof(VipManager)).AddComponent<VipManager>();
            instance.gameObject.AddComponent<AudioSource>();
            instance.musicClient = musicClient;
        }
    }
}