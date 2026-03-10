using System.Collections.Generic;
using System.Linq;
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
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Constants = Assets.Scripts.Services.Constants;
using Image = UnityEngine.UI.Image;

public class MainMenuManager : UnityEventUser
{
    private Room _room;
    private MusicClient _musicClient;
    private AudioSource _audioSource;
    private AppSettings _appSettings;

    private readonly List<PlayerScript> _playerScripts = new();

    public GameObject InstructionsContainer;
    public GameObject PlayersContainer;
    public TextMeshProUGUI InstructionsWebUrl;
    public TextMeshProUGUI RoomCode;
    public Button StartButton;
    public Button OptionsButton;
    public Image QRCodeImage;
    public Animator ArmAnimator;
    public AudioSource Music;

    // Start is called before the first frame update
    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();

        StartButton.interactable = false;
        OptionsButton.interactable = false;
        StartButton.Select();

        _room = ServiceProvider.Get<Room>();
        _appSettings = ServiceProvider.Get<AppSettings>();
        _musicClient = ServiceProvider.Get<MusicClient>();
        InstructionsContainer.gameObject.SetActive(false);
        var gameOptions = ServiceProvider.Get<GameOptions>();
        gameOptions.ApplyVolume();
        if (gameOptions.MenuMusic)
            Music.PlayOneShot(Constants.AudioClips.GetRandomMenuMusic());
        //ServiceProvider.Get<CustomEventService>().TryPushRemainingEventsAsync().CatchErrors();
        NewsPanelScript.AutoCheckAsync().CatchErrors();
        LoadUI();
    }

    private void LoadUI()
    {
        OptionsButton.interactable = true;
        RoomCode.text = _room.Code;
        InstructionsWebUrl.text = _appSettings.WebClientUri.Host;
        QRCodeImage.sprite = QRUtility.GenerateQrCodeTexture($"{_appSettings.WebClientUrl}/?code={_room.Code}").ToSprite();
        QRCodeImage.gameObject.SetActive(true);
        QRCodeImage.preserveAspect = true;
        QRCodeImage.color = Constants.Colors.Accent3;

        InstructionsContainer.SetActive(true);

        InstantiatePlayers();

        foreach (var player in _room.Players) AddOrUpdatePlayer(player);

        AddListenersForMusicClientEvents();
    }

    private void InstantiatePlayers()
    {
        foreach (var character in _appSettings.AvailableCharacters) _playerScripts.Add(PlayerScript.Instantiate(PlayersContainer.transform, character));
        for (var i = 0; i < 8; i++)
        {
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (EventSystem.current.currentSelectedGameObject == null &&
            transform.childCount == 2)
            StartButton.Select();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S)) SceneFader.Fade("SpeedRound", Color.black, 2);
#endif
    }

    private void OnDestroy()
    {
        _musicClient.RemoveAllListenersFrom(this);
    }

    private void AddOrUpdatePlayer(Player player)
    {
        if (!_room.Players.Contains(player)) _room.Players.Add(player);

        if (!_playerScripts.ContainsPlayer(player.Id))
        {
            var playerScript = _playerScripts.First(ps => !ps.PlayerLoaded);
            playerScript.LoadPlayer(player);
            playerScript.AnimateJoinAsync().CatchErrors();
            _audioSource.Play();
        }
        else
        {
            _playerScripts.Find(ps => string.Equals(ps.Id, player.Id))
                .ReloadPlayer(player);
        }

        if (_room.Players.Count > 1 && !StartButton.interactable)
        {
            StartButton.interactable = true;
            StartButton.Select();
            StartButton.OnSelect(new BaseEventData(EventSystem.current));
            ArmAnimator.Play(Constants.Animations.MoveTurntableArm);
        }

        _musicClient.ScheduleActionAsync(System.TimeSpan.FromMilliseconds(300), ServerMethods.Relax, new RelaxMessage
        {
            PlayerOrGuestIds = new List<string> { player.Id },
            RoomCode = player.Code
        }).CatchErrors();
    }

    public void Exit() => Application.Quit();

    public void OpenOptions() =>
        OptionsPanelScript.Instantiate().MenuMusicToggle.onValueChanged.AddListener(isOn =>
        {
            if (isOn) Music.PlayOneShot(Constants.AudioClips.GetRandomMenuMusic());
            else Music.Stop();
        });

    public async void StartNewGame()
    {
        try
        {
            this.DisableAllButtons();
            LoadingSpinner.Instantiate(transform);
            var options = ServiceProvider.Get<GameOptions>();
            var room = ServiceProvider.Get<Room>();
            var response = await _musicClient.StartNewGameAsync(
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
        _musicClient.PlayerRejoined.AddListener(
            this,
            player => AddOrUpdatePlayer(player));

        _musicClient.Message.AddListener(
            this,
            message => ToastPanelScript.Instantiate(message));

        _musicClient.PlayerJoined.AddListener(this, newPlayer => AddOrUpdatePlayer(newPlayer));
    }
}