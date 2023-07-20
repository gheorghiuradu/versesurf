using Assets.Scripts.Extensions;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain.Domain;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace Assets.Scripts.Reusable
{
    [RequireComponent(typeof(EventTrigger), typeof(NicerOutline))]
    public class PlaylistButton : MonoBehaviour
    {
        private const string PrefabPath = "Prefabs/PlaylistButton";
        private const string SelectedAnimation = "PlaylistButtonSelected";

        [SerializeField]
        private Image AlbumCover;

        [SerializeField]
        private TextMeshProUGUI PlaylistNameTMP;

        [SerializeField]
        private GameObject ToolTip;

        [SerializeField]
        private NicerOutline nicerOutline;

        [SerializeField]
        private Animator animator;

        [SerializeField]
        private AudioSource audioSource;

        private UnityAction<string> onClick;
        private Material defaultMaterial;

        public string PlaylistName { get; private set; }
        public bool PlaylistEnabled { get; private set; }
        public string ToolTipText { get; private set; }
        public string PlaylistId { get; private set; }
        public string PlaylistPictureUrl { get; private set; }
        public string PlaylistPictureHash { get; private set; }
        public string PlaylistKeyWords { get; private set; }

        private void Start()
        {
            this.defaultMaterial = this.AlbumCover.material;
            this.PlaylistNameTMP.text = this.PlaylistName;
            if (!string.IsNullOrEmpty(this.PlaylistPictureUrl) && !string.IsNullOrEmpty(this.PlaylistPictureHash))
            {
                this.LoadAlbumImageAsync();
            }
            if (!this.PlaylistEnabled)
            {
                this.ToggleGrayScale();
            }
            this.ToolTip.GetComponentInChildren<TextMeshProUGUI>().text = this.ToolTipText;
        }

        private async void LoadAlbumImageAsync()
        {
            var cacheSerice = ServiceProvider.Get<CacheService>();
            this.AlbumCover.sprite = await cacheSerice.HandlePlaylistImageAsync(this.PlaylistPictureUrl, this.PlaylistPictureHash);
        }

        private void ToggleGrayScale()
        {
            if (this.AlbumCover.material == this.defaultMaterial)
            {
                this.AlbumCover.material = Constants.Materials.GetGrayScale();
            }
            else
            {
                this.AlbumCover.material = this.defaultMaterial;
            }
        }

        public void OnSelected(BaseEventData eventData)
        {
            this.nicerOutline.enabled = true;
            if (!string.IsNullOrEmpty(this.ToolTipText))
            {
                this.ToolTip.SetActive(true);
            }
        }

        public void OnDeselected(BaseEventData eventData)
        {
            this.nicerOutline.enabled = false;
            if (this.ToolTip.activeSelf)
            {
                this.ToolTip.SetActive(false);
            }
        }

        public void OnClick(BaseEventData eventData)
        {
            this.onClick?.Invoke(this.PlaylistId);
        }

        public async Task AnimateSelectedAsync()
        {
            this.nicerOutline.enabled = false;
            var rectTransform = this.GetComponent<RectTransform>();
            rectTransform.AnimateMoveTowardsAsync(Camera.main.GetScreenCenter(), 1.5f, 15).CatchErrors();
            while (!rectTransform.IsVisibleFrom(Camera.main))
            {
                await new WaitForSeconds(0.25f);
            }
            ServiceProvider.Get<GameOptions>().ApplyVolume(this.gameObject);
            this.audioSource.Play();
            await new WaitForSeconds(0.25f);

            var clip = Array.Find(this.animator.runtimeAnimatorController.animationClips, c => c.name.Equals(SelectedAnimation));
            this.animator.Play(SelectedAnimation);
            await new WaitForSeconds(clip.length);
        }

        public static PlaylistButton Instantiate(
            IPlaylistViewModel playlistViewModel,
            Transform parent,
            UnityAction<string> onClick = null,
            bool enabled = false,
            string tooltipText = null)
        {
            var script = InstantiateScript(parent);
            script.PlaylistId = playlistViewModel.Id;
            script.PlaylistName = playlistViewModel.Name;
            script.PlaylistEnabled = enabled;
            script.ToolTipText = tooltipText;
            script.PlaylistPictureUrl = playlistViewModel.PictureUrl;
            script.PlaylistPictureHash = playlistViewModel.PictureHash;
            script.PlaylistKeyWords = (playlistViewModel as PlaylistViewModel)?.KeyWords;
            script.onClick = onClick;

            return script;
        }

        private static PlaylistButton InstantiateScript(Transform parent)
        {
            var prefab = Resources.Load<GameObject>(PrefabPath);
            return GameObject.Instantiate(prefab, parent).GetComponent<PlaylistButton>();
        }
    }
}