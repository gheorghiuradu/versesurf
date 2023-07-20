using Assets.Scripts.Extensions;
using Assets.Scripts.News;
using Assets.Scripts.Options;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;
using SharedDomain.Messages.Commands;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Constants = Assets.Scripts.Services.Constants;

public class MainMenuManager : UnityEventUser
{
    private Room room;
    private MusicClient musicClient;
    private AudioSource audioSource;
    private AppSettings appSettings;

    private List<PlayerScript> playerScripts = new List<PlayerScript>();

    public GameObject InstructionsContainer;
    public GameObject PlayersContainer;
    public TextMeshProUGUI InstructionsWebUrl;
    public TextMeshProUGUI RoomCode;
    public Button StartButton;
    public Button StoreButton;
    public Button OptionsButton;
    public Image QRCodeImage;
    public Animator ArmAnimator;
    public AudioSource Music;

    // Start is called before the first frame update
    private void Start()
    {
        this.audioSource = this.GetComponent<AudioSource>();

        this.StartButton.interactable = false;
        this.OptionsButton.interactable = false;
        this.StoreButton.interactable = false;
        this.StartButton.Select();

        this.room = ServiceProvider.Get<Room>();
        this.appSettings = ServiceProvider.Get<AppSettings>();
        this.musicClient = ServiceProvider.Get<MusicClient>();
        this.InstructionsContainer.gameObject.SetActive(false);
        var gameOptions = ServiceProvider.Get<GameOptions>();
        gameOptions.ApplyVolume();
        if (gameOptions.MenuMusic)
            this.Music.PlayOneShot(Constants.AudioClips.GetRandomMenuMusic());
        //ServiceProvider.Get<CustomEventService>().TryPushRemainingEventsAsync().CatchErrors();
        NewsPanelScript.AutoCheckAsync().CatchErrors();
        this.LoadUI();
    }

    private void LoadUI()
    {
        this.OptionsButton.interactable = true;
        this.StoreButton.interactable = true;
        this.RoomCode.text = this.room.Code;
        this.InstructionsWebUrl.text = this.appSettings.WebClientUri.Host;
        this.QRCodeImage.sprite = QRUtility.GenerateQrCodeTexture($"{appSettings.WebClientUrl}/?code={this.room.Code}").ToSprite();
        this.QRCodeImage.gameObject.SetActive(true);
        this.QRCodeImage.preserveAspect = true;
        this.QRCodeImage.color = Constants.Colors.Accent1;

        this.InstructionsContainer.SetActive(true);

        this.InstantiatePlayers();

        foreach (var player in this.room.Players)
        {
            this.AddOrUpdatePlayer(player);
        }

        this.AddListenersForMusicClientEvents();
    }

    private void InstantiatePlayers()
    {
        foreach (var character in this.appSettings.AvailableCharacters)
        {
            this.playerScripts.Add(PlayerScript.Instantiate(this.PlayersContainer.transform, character));
        }
        for (int i = 0; i < 8; i++)
        {
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null &&
            this.transform.childCount == 2)
        {
            this.StartButton.Select();
        }

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneFader.Fade("SpeedRound", Color.black, 2);
        }
#endif
    }

    private void OnDestroy()
    {
        this.musicClient.RemoveAllListenersFrom(this);
    }

    private void AddOrUpdatePlayer(Player player)
    {
        if (!this.room.Players.Contains(player))
        {
            this.room.Players.Add(player);
        }

        if (!this.playerScripts.ContainsPlayer(player.Id))
        {
            var playerScript = this.playerScripts.First(ps => !ps.PlayerLoaded);
            playerScript.LoadPlayer(player);
            playerScript.AnimateJoinAsync().CatchErrors();
            this.audioSource.Play();
        }
        else
        {
            this.playerScripts.Find(ps => string.Equals(ps.Id, player.Id))
                .ReloadPlayer(player);
        }

        if (this.room.Players.Count > 1 && !this.StartButton.interactable)
        {
            this.StartButton.interactable = true;
            this.StartButton.Select();
            this.StartButton.OnSelect(new BaseEventData(EventSystem.current));
            this.ArmAnimator.Play(Constants.Animations.MoveTurntableArm);
        }

        this.musicClient.ScheduleActionAsync(System.TimeSpan.FromMilliseconds(300), ServerMethods.Relax, new RelaxMessage
        {
            PlayerOrGuestIds = new List<string> { player.Id },
            RoomCode = player.Code
        }).CatchErrors();
    }

    public void Exit() => Application.Quit();

    public void OpenStore() => StoreManagerScript.Instantiate();

    public void OpenOptions() =>
        OptionsPanelScript.Instantiate().MenuMusicToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) this.Music.PlayOneShot(Constants.AudioClips.GetRandomMenuMusic());
            else this.Music.Stop();
        });

    public async void StartNewGame()
    {
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
            ErrorPanelScript.Instantiate("Failed to start game.");
        }
    }

    public void OpeTutorial() => TutorialPanelScript.Instantiate();

    private void AddListenersForMusicClientEvents()
    {
        this.musicClient.PlayerRejoined.AddListener(
            this,
            player => this.AddOrUpdatePlayer(player));

        this.musicClient.Message.AddListener(
            this,
            message => ToastPanelScript.Instantiate(message));

        this.musicClient.PlayerJoined.AddListener(this, newPlayer => this.AddOrUpdatePlayer(newPlayer));
    }
}