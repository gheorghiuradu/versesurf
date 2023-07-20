using Assets.Scripts.Extensions;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.Panels
{
    public class SelectPassPanelScript : MonoBehaviour
    {
        private const string SelectPassPanelPrefabPath = "Prefabs/SelectPassPanel";

        private PlayFabService playFabService;
        private MusicClient musicClient;
        private Room room;

        public Transform PassGrid;

        private void Start()
        {
            this.playFabService = ServiceProvider.Get<PlayFabService>();
            this.musicClient = ServiceProvider.Get<MusicClient>();
            this.room = ServiceProvider.Get<Room>();
            this.LoadInventoryItemsAsync().CatchErrors();
        }

        private void LateUpdate()
        {
            if (!this.isActiveAndEnabled)
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) && this.GetComponent<RectTransform>().IsFullyVisibleFrom(Camera.main))
            {
                this.Close();
            }

            if (EventSystem.current.currentSelectedGameObject == null &&
                this.transform.IsLastChild() &&
                this.PassGrid.childCount > 0)
            {
                var firstProductButton = this.PassGrid.GetComponentInChildren<Button>();
                firstProductButton.Select();
            }

            if (this.room?.Players.Count < 2)
            {
                this.Close();
            }
        }

        public void Close()
        {
            GameObject.Destroy(this.gameObject);
        }

        public static void Instantiate()
        {
            var canvas = GameObject.FindObjectOfType<Canvas>();
            var prefab = Resources.Load<GameObject>(SelectPassPanelPrefabPath);

            GameObject.Instantiate(prefab, canvas.transform).transform.SetAsLastSibling();
        }

        public async void SelectPass(string itemInstanceId)
        {
            if (itemInstanceId is null)
            {
                return;
            }

            try
            {
                this.DisableAllButtons();
                LoadingSpinner.Instantiate(this.transform);
                var options = ServiceProvider.Get<GameOptions>();
                var room = ServiceProvider.Get<Room>();
                var response = await this.musicClient.StartNewGameAsync(
                        options.PlaylistOptions);

                if (!response.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(response.ErrorMessage);
                    return;
                }
                ServiceProvider.AddOrReplace(response.GetData<List<PlaylistViewModel>>());
                SceneFader.Fade("PlaylistVotingBooth", Color.black, 2);
            }
            catch (System.Exception)
            {
                this.EnableAllButtons();
                LoadingSpinner.Destroy();
                ErrorPanelScript.Instantiate("Failed to activate that pass.");
            }
        }

        private async Task LoadInventoryItemsAsync()
        {
            LoadingSpinner.Instantiate(this.transform);
            var inventory = await this.playFabService.GetInventoryItemsAsync();

            InventoryItemScript.InstantiateCustom("Free game", string.Empty, "∞", this.PassGrid, this.SelectPass);
            foreach (var item in inventory)
            {
                InventoryItemScript.Instantiate(item, this.PassGrid, this.SelectPass);
            }
            InventoryItemBlackScript.Instantiate("Go to store", this.PassGrid, () =>
            {
                StoreManagerScript.Instantiate();
                this.Close();
            });
            LoadingSpinner.Destroy();
            this.PassGrid.GetComponentInChildren<Button>().Select();
        }
    }
}