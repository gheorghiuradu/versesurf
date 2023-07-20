using Assets.Scripts.Extensions;
using Assets.Scripts.Reusable;
using Assets.Scripts.Services;
using SharedDomain;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Panels
{
    public class KickPlayerPanel : MonoBehaviour, IClosablePanel
    {
        private const string PrefabPath = "Prefabs/KickPlayerPanel";
        private const int AdditionalHeight = 228;

        private MusicClient musicClient;
        private Action onClose;
        private readonly UnityEvent<string> playerKicked = new PlayerKicked();

        public Transform PlayerContainer;

        private void Start()
        {
            this.musicClient = ServiceProvider.Get<MusicClient>();
            var players = ServiceProvider.Get<Room>().Players;

            foreach (var player in players)
            {
                PlayerButtonScript.Instantiate(player, this.PlayerContainer, this.KickPlayer);
            }

            var playerPrefabHeight = Resources.Load<GameObject>(PlayerButtonScript.PrefabPath)
                .GetComponent<RectTransform>().sizeDelta.y;
            var requiredHeight = (players.Count * playerPrefabHeight) + AdditionalHeight;
            this.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, requiredHeight);

            this.PlayerContainer.GetComponentInChildren<Button>().Select();
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null &&
                this.transform.IsLastChild())
            {
                this.PlayerContainer.GetComponentInChildren<Button>().Select();
            }
        }

        public void Close()
        {
            GameObject.Destroy(this.gameObject);
        }

        private void OnDestroy()
        {
            this.onClose?.Invoke();
        }

        public async void KickPlayer()
        {
            if (EventSystem.current.currentSelectedGameObject != null &&
                EventSystem.current.currentSelectedGameObject.GetComponent<PlayerButtonScript>() != null)
            {
                LoadingSpinner.Instantiate();
                var playerScript = EventSystem.current.currentSelectedGameObject.GetComponent<PlayerButtonScript>();
                this.DisableAllButtons();
                try
                {
                    var result = await this.musicClient.KickPlayerAsync(playerScript.Player.Id);
                    if (result.IsSuccess)
                    {
                        this.playerKicked.Invoke(playerScript.Player.Id);
                    }
                    else
                    {
                        ErrorPanelScript.Instantiate(result.ErrorMessage);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    ErrorPanelScript.Instantiate("Sorry, something went wrong.");
                }
                finally
                {
                    LoadingSpinner.Destroy();
                    this.Close();
                }
            }
        }

        public static KickPlayerPanel Instantiate(Transform parent, Action onClose = null, IEnumerable<UnityAction<string>> onPlayerKicked = null)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            var instance = GameObject.Instantiate(prefab, parent).GetComponent<KickPlayerPanel>();
            instance.onClose = onClose;
            if (!(onPlayerKicked is null))
            {
                foreach (var action in onPlayerKicked)
                {
                    instance.playerKicked.AddListener(action);
                }
            }

            return instance;
        }

        private class PlayerKicked : UnityEvent<string> { }
    }
}