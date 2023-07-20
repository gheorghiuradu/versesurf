using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using Assets.Scripts.VIP;
using SharedDomain;
using SharedDomain.Domain;
using SharedDomain.Messages.Commands;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.PlaylistVotingBooth
{
    public class PlaylistVotingBoothManager : UnityEventUser
    {
        private List<PlaylistButton> playlistScripts = new List<PlaylistButton>();
        private List<PlayerScript> playerScripts = new List<PlayerScript>();

        private MusicClient musicClient;
        private Room room;
        private AudioSource audioSource;
        private GameOptions gameOptions;
        private GridLayoutGroup playlistGrid;
        private VipManager vipManager;

        public Transform PlaylistLayout;
        public Transform PlayersContainer;
        public Transform GamePanel;

        private void Start()
        {
            LoadingSpinner.Instantiate(this.GamePanel);

            this.musicClient = ServiceProvider.Get<MusicClient>();
            this.room = ServiceProvider.Get<Room>();
            this.audioSource = this.GetComponent<AudioSource>();
            this.playlistGrid = this.PlaylistLayout.GetComponent<GridLayoutGroup>();
            this.vipManager = GameObject.FindObjectOfType<VipManager>();
            this.gameOptions = ServiceProvider.Get<GameOptions>();
            this.BindToMusicClientEvents();
            this.vipManager.VipActivated.AddListener(this, this.LoadPlaylists);

            foreach (var player in this.room.Players)
            {
                var playerScript = PlayerScript.Instantiate(this.PlayersContainer.transform, player, ActionState.Pending);
                this.playerScripts.Add(playerScript);
            }

            this.gameOptions.ApplyVolume();
            try
            {
                this.LoadPlaylists();
                this.playlistGrid.ResizeHeightToFitChildren();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
                ErrorPanelScript.Instantiate("Sorry, something went wrong. Please try again or check your connection.");
            }

            if (this.gameOptions.MenuMusic)
            {
                this.audioSource.loop = true;
                this.audioSource.clip = Constants.AudioClips.GetRandomBgMusic();
                this.audioSource.Play();
            }
            LoadingSpinner.Destroy();
        }

        private void Update()
        {
            if (EventSystem.current.currentSelectedGameObject == null &&
                this.transform.childCount == 2)
            {
                var firstPlaylist = this.playlistScripts?[0];
                if (firstPlaylist?.isActiveAndEnabled == true)
                {
                    EventSystem.current.SetSelectedGameObject(firstPlaylist.gameObject);
                }
            }

            if (this.transform.childCount == 2 && Input.GetKeyDown(KeyCode.Escape)
                && EventSystem.current.currentSelectedGameObject?.GetComponent<TMP_InputField>() == null)
            {
                SimplePausePanel.Instantiate(this.transform, false, onExit: this.OnClickBack);
            }
        }

        private async void LoadRoundAsync(string selectedPlaylistId)
        {
            try
            {
                this.DisableAllButtons();
                LoadingSpinner.Instantiate("Preparing playlist, please wait...", this.GamePanel);
                var fullPlaylistResponse = await this.musicClient.GetFullPlaylistAsync(selectedPlaylistId, this.gameOptions.PlaylistOptions);

                if (!fullPlaylistResponse.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(fullPlaylistResponse.ErrorMessage);
                    return;
                }
                var fullPlaylist = fullPlaylistResponse.GetData<FullPlaylistViewModel>();
                fullPlaylist.Songs.Shuffle();
                this.room.NewGame(fullPlaylist);
                this.audioSource.FadeOut().CatchErrors();

                //Get selected playlist and detach it from grid, attach it to canvas
                var selectedPlaylist = this.playlistScripts.Find(ps => ps.PlaylistId.Equals(fullPlaylist.Id));
                selectedPlaylist.transform.SetParent(this.transform);
                //Fade all other elements
                foreach (var image in this.GamePanel.GetComponentsInChildren<Image>().Except(this.GamePanel.GetComponent<Image>()))
                {
                    image.CrossFadeAlpha(0, 1, false);
                }
                foreach (var tmPro in this.GamePanel.GetComponentsInChildren<TextMeshProUGUI>())
                {
                    tmPro.gameObject.SetActive(false);
                }

                //Animate the selected playlist
                await selectedPlaylist.AnimateSelectedAsync();
                await new WaitForSeconds(0.4f);

                var nextScene = this.gameOptions.NumberOfRounds == 1 ?
                    "SpeedRound" : "Round";
                SceneFader.Fade(nextScene, Color.black, 2);
            }
            catch (System.Exception ex)
            {
                LoadingSpinner.Destroy();
                Debug.LogError(ex);
                ErrorPanelScript.Instantiate("There was an error starting the game, please try again later.");
            }
            finally
            {
                this.EnableAllButtons();
            }
        }

        private void BindToMusicClientEvents()
        {
            this.musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));
        }

        private void OnDestroy()
        {
            this.musicClient.RemoveAllListenersFrom(this);
            this.vipManager.RemoveAllListenersFrom(this);
        }

        public void OnSelectPlaylist(string selectedId)
        {
            this.musicClient.RelaxAsync(this.room.Players.Select(p => p.Id)).CatchErrors();
            this.LoadRoundAsync(selectedId);
        }

        public async void OnClickBack()
        {
            try
            {
                var quitGameResult = await this.musicClient.QuitGameAsync();
                if (!quitGameResult.IsSuccess)
                {
                    Debug.LogError(quitGameResult.ErrorMessage);
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex);
            }
        }

        public void SearchPlaylists(string query)
        {
            var playlists = this.playlistScripts.Where(ps => !string.IsNullOrWhiteSpace(ps.PlaylistKeyWords) &&
                (ps.PlaylistKeyWords.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                ps.PlaylistName.IndexOf(query, System.StringComparison.OrdinalIgnoreCase) >= 0));
            foreach (var item in playlists)
            {
                item.gameObject.SetActive(true);
            }
            foreach (var item in this.playlistScripts.Except(playlists))
            {
                item.gameObject.SetActive(false);
            }
            this.playlistScripts.Find(ps => string.Equals(ps.PlaylistName, "Random"))?.gameObject.SetActive(true);
        }

        private void LoadPlaylists()
        {
            //Destroy previous playlist buttons
            foreach (var playlistVm in this.playlistScripts)
            {
                GameObject.Destroy(playlistVm.gameObject);
            }
            this.playlistScripts.Clear();

            //Prepare actions, UI
            var isVip = this.vipManager.VipPerks.Contains(VipPerk.SelectPlaylist);
            var toolTipText = isVip ? string.Empty : "Only VIP players can select playlist";
            UnityAction<string> onClick = null;
            if (isVip)
            {
                onClick = this.OnSelectPlaylist;
            }
            else
            {
                onClick = _ => StoreManagerScript.Instantiate();
            }

            //Add random button
            this.playlistScripts.Add(PlaylistButton.Instantiate(
                new PlaylistViewModel { Id = string.Empty, Name = "Random" },
                this.PlaylistLayout,
                this.OnSelectPlaylist,
                true));

            //Create new playlist buttons
            foreach (var playlist in ServiceProvider.Get<List<PlaylistViewModel>>())
            {
                this.playlistScripts.Add(PlaylistButton.Instantiate(
                    playlist,
                    this.PlaylistLayout,
                    onClick,
                    isVip,
                    toolTipText
                    ));
            }
        }
    }
}