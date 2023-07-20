using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Services;
using PlayFab.ClientModels;
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
        private PlayFabService playFab;

        private AudioSource audioSource;

        private ItemInstance currentItem;
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

            if (this.currentItem != null)
            {
                this.TryNotifyExpiration();
            }
        }

        private void TryNotifyExpiration()
        {
            if (this.currentItem?.Expiration.HasValue == false)
            {
                return;
            }

            if (!this.hasNotifiedExpiration && this.currentItem.Expiration <= DateTime.UtcNow.AddMinutes(5))
            {
                ToastPanelScript.Instantiate(new NotificationMessage { Title = "VIP Status", Message = "Your VIP membership expires soon" });
                this.hasNotifiedExpiration = true;
            }
        }

        private void OnGameEnded()
        {
            if (this.IsVip && this.currentItem.Expiration <= DateTime.UtcNow)
            {
                this.DiscardVipAssets();
                ToastPanelScript.Instantiate(new NotificationMessage { Title = "VIP Status", Message = "Your VIP membership has expired :(" });
                this.audioSource.PlayOneShot(Constants.AudioClips.GetVipDiscardedSound());
            }
        }

        public async Task AttemptAutoActivation()
        {
            if (!this.hasAttemptedAutoActivation && !this.IsVip)
            {
                var inventory = await this.playFab.GetInventoryItemsAsync();
                if (inventory.Count == 0)
                {
                    this.hasAttemptedAutoActivation = true;
                    return;
                }

                var result = await this.TryActivateItem(inventory[0]);
                if (!result)
                {
                    this.hasAttemptedAutoActivation = true;
                    return;
                }
                this.ApplyVipAssets();
                this.audioSource.PlayOneShot(Constants.AudioClips.GetVipActivatedSound());
                this.hasNotifiedExpiration = false;
            }
        }

        public async Task<bool> TryActivateItem(ItemInstance item)
        {
            var defaultPerks = new List<VipPerk> { VipPerk.SelectPlaylist };
            var result = await this.musicClient.ActivateVipAsync(item.ItemInstanceId, defaultPerks);
            if (!result.IsSuccess)
            {
                return false;
            }

            this.currentItem = item;
            foreach (var perk in defaultPerks)
            {
                this.VipPerks.Add(perk);
            }

            this.VipActivated.Invoke();
            return true;
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

        public static void Initialize(MusicClient musicClient, PlayFabService playFab)
        {
            var instance = new GameObject(nameof(VipManager)).AddComponent<VipManager>();
            instance.gameObject.AddComponent<AudioSource>();
            instance.musicClient = musicClient;
            instance.playFab = playFab;
        }
    }
}