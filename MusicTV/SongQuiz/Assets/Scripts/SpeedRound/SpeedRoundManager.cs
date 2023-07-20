using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Round;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.SpeedRound
{
    public class SpeedRoundManager : UnityEventUser
    {
        private const float SecondsToAnswer = 36;

        [SerializeField]
        private Animator speedRoundLogo;

        [SerializeField]
        private TextMeshProUGUI instructions;

        [SerializeField]
        private TextMeshProUGUI instructionsHeader;

        [SerializeField]
        private TextMeshProUGUI instructionsListen;

        [SerializeField]
        private PlaylistScript playlistScript;

        [SerializeField]
        private TextMeshProUGUI timerTMP;

        [SerializeField]
        private GameObject timerThunder;

        [SerializeField]
        private AudioSource sfx;

        [SerializeField]
        private AudioSource music;

        [SerializeField]
        private AudioSource bgMusic;

        [SerializeField]
        private GameObject roundNumberPanel;

        [SerializeField]
        private GameObject answersContainer;

        [SerializeField]
        private RectTransform answerContainerPlaceholder;

        private MusicClient musicClient;
        private GameOptions gameOptions;

        private ConcurrentDictionary<string, SharedDomain.SpeedAnswer> answers;

        private Timer timer;
        private Room room;
        private bool isPaused;
        private float secondsToPlay;
        private Snippet snippet;

        // Awake is called first
        private void Awake()
        {
            this.timer = new Timer();
            this.room = ServiceProvider.Get<Room>();
            this.musicClient = ServiceProvider.Get<MusicClient>();
            this.gameOptions = ServiceProvider.Get<GameOptions>();
            this.answers = new ConcurrentDictionary<string, SharedDomain.SpeedAnswer>();

            this.timer.OnStep(2, () => { this.bgMusic.Stop(); this.music.UnPause(); this.music.FadeOut().CatchErrors(); this.ShowAnswersAsync().CatchErrors(); });

            var bellSound = Resources.Load<AudioClip>(Constants.BellSmallMutedSoundPath);
            var cinematicBoomSound = Resources.Load<AudioClip>(Constants.CinematicBoomSoundPath);
            this.timer.OnLastSeconds(10, () => this.sfx.PlayOneShot(bellSound));
            this.timer.OnLastSeconds(1, () => this.sfx.PlayOneShot(cinematicBoomSound));
            this.BindToMusicClient();
        }

        //Start is called after Awake
        private async void Start()
        {
            this.roundNumberPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Round {this.room.CurrentRound.Number}";
            var song = this.room.Playlist.Songs[this.room.CurrentRound.Number - 1];
            this.playlistScript.InitializeAsync(this.room.Playlist, song).CatchErrors();
            this.music.clip = await ServiceProvider.Get<CacheService>().HandleSongAsync(song.PreviewUrl, song.PreviewHash);
            this.gameOptions.ApplyVolume();

            this.snippet = song.Snippet.Contains("{") ?
                    new Snippet(song.Snippet) : new Snippet(song.Snippet, Snippet.Mode.LastTwoWords);

            // Show Round number for at least 1 second
            if (Time.timeSinceLevelLoad < 1)
            {
                await new WaitForSeconds(1 - Time.timeSinceLevelLoad);
            }
            GameObject.Destroy(this.roundNumberPanel);
            this.speedRoundLogo.gameObject.SetActive(true);
            await new WaitForSeconds(this.speedRoundLogo.runtimeAnimatorController.animationClips[0].length + 1.5f);
            this.speedRoundLogo.gameObject.SetActive(false);

            this.instructionsListen.gameObject.SetActive(true);
            this.secondsToPlay = song.EndSecond.HasValue ?
                song.EndSecond.Value - song.StartSecond.GetValueOrDefault() : this.gameOptions.SongPlayLengthSeconds;
            this.music.Play();

            while (this.music.time <= this.secondsToPlay)
            {
                await new WaitForSeconds(0.01f);
            }

            this.music.Pause();
            this.instructionsListen.gameObject.SetActive(false);
            await this.LoadAnswersContainerAsync();
            this.instructions.text = this.snippet.Question;
            this.instructionsHeader.gameObject.SetActive(true);
            this.instructions.gameObject.SetActive(true);

            if (this && !this.isPaused)
            {
                await this.musicClient.AskSpeedAsync(song.Id);
                this.timer.StartCountdown(SecondsToAnswer);
                this.PlayBgMusic();
            }
        }

        private void LateUpdate()
        {
            this.timerTMP.text = this.timer.Text;
            this.timer.Tick(Time.deltaTime);
            this.timerThunder.SetActive(this.timer.Enabled);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.Pause();
            }
        }

        private void OnDestroy()
        {
            this.musicClient.RemoveAllListenersFrom(this);
        }

        private void Pause()
        {
            if (this.isPaused)
            {
                return;
            }

            this.isPaused = true;
            Time.timeScale = 0;
            var pauseScript = PausePanelScript.Instantiate(onPlayerKicked: this.OnPlayerKicked);
            pauseScript.ShouldRestart = this.music.time < this.secondsToPlay;
            if (this.music.isPlaying) this.music.Pause();
            this.bgMusic.Pause();
            if (this.timer.Enabled)
            {
                this.timer.Pause();
                pauseScript.OnResume = () => { this.timer.Resume(); this.isPaused = false; Time.timeScale = 1; this.bgMusic.UnPause(); };
            }
            else
            {
                pauseScript.OnResume = () => { this.isPaused = false; Time.timeScale = 1; this.bgMusic.UnPause(); };
            }
            pauseScript.OnExitAsync = async () =>
            {
                var quitGameResult = await this.musicClient.QuitGameAsync();
                if (!quitGameResult.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(quitGameResult.ErrorMessage);
                }
            };
        }

        private void OnPlayerKicked(string playerId)
        {
            var foundPlayer = this.room.KickPlayer(playerId);
            if (foundPlayer)
            {
                var answer = GameObject.Find(playerId);
                if (answer)
                {
                    GameObject.Destroy(answer);
                }
                this.sfx.PlayOneShot(Constants.AudioClips.GetPlayerDisconnectedSound());
            }
        }

        private async Task ShowAnswersAsync()
        {
            foreach (var speedAnswer in this.answersContainer.GetComponentsInChildren<SpeedAnswer>())
            {
                await speedAnswer.RevealAsync();
                await new WaitForSeconds(0.7f);
            }
            await new WaitForSeconds(2.2f);

            SceneFader.Fade("Score", Color.black, 2f);
        }

        private void BindToMusicClient()
        {
            this.musicClient.Disconnected.AddListener(this, error => Debug.LogException(error));
            this.musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));
            this.musicClient.SpeedAnswered.AddListener(this, answer =>
            {
                this.musicClient.RelaxAsync(answer.Player.Id).CatchErrors();

                answer.Normalize(this.snippet.Answer);
                this.answers.TryAdd(answer.Player.Id, answer);
                var answerTime = Mathf.CeilToInt(SecondsToAnswer - this.timer.Time);
                var isCorrect = string.Equals(answer.Name, this.snippet.Answer, StringComparison.OrdinalIgnoreCase);
                if (isCorrect)
                    this.room.CurrentRound.Score
                    .AddScore(answer.Player, Mathf.CeilToInt(this.timer.Time) * Constants.SpeedAnswerPointsMultiplier);

                SpeedAnswer.Instantiate(this.answersContainer.transform, answer, answerTime, isCorrect);

                if (this.answers.Count == this.room.Players.Count)
                {
                    this.timer.SkipStep();
                }
            });
        }

        private async Task LoadAnswersContainerAsync()
        {
            var original = this.answersContainer.GetChildren().First();
            for (int i = 0; i < this.room.Players.Count - 1; i++)
            {
                GameObject.Instantiate(original.gameObject, this.answersContainer.transform);
            }

            this.sfx.PlayOneShot(Constants.AudioClips.GetMetalMoveSound());
            await this.answersContainer.GetComponent<RectTransform>()
                .AnimateMoveTowardsAsync(this.answerContainerPlaceholder.position, 2f, 3f);
        }

        private void PlayBgMusic()
        {
            if (this.gameOptions.MenuMusic)
            {
                this.bgMusic.loop = true;
                this.bgMusic.clip = Constants.AudioClips.GetRandomBgMusic();
                this.bgMusic.Play();
            }
        }
    }
}