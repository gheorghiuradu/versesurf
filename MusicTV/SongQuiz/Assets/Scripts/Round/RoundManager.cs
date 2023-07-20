using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.Round
{
    public class RoundManager : UnityEventUser
    {
        public RectTransform InstructionsContainer;
        public TextMeshProUGUI Instructions;
        public TextMeshProUGUI InstructionsHeader;
        public TextMeshProUGUI InstructionsListen;
        public PlaylistScript PlaylistScript;
        public GameObject AnswersContainer;
        public TextMeshProUGUI AnswersContainerHeader;
        public TextMeshProUGUI TimerTMP;
        public AudioSource PanelAudio;
        public GameObject PlayersContainer;
        public GameObject RoundNumberPanel;
        public AudioSource BgMusic;

        private MusicClient musicClient;
        private GameOptions gameOptions;

        private ConcurrentDictionary<string, Answer> answers;
        private List<AnswerScript> UIAnswers;
        private List<PlayerScript> playerScripts;

        private Timer timer;
        private Snippet snippet;
        private Answer correctAnswer;
        private Room room;
        private bool isPaused;
        private float secondsToPlay;

        private AudioClip uiMenuButtonScroll01Sound;
        private AudioSource audioSource;

        // Awake is called first
        private void Awake()
        {
            this.correctAnswer = new Answer { Id = Guid.NewGuid().ToString(), Player = new Player { Id = "CORRECT" } };
            this.timer = new Timer();
            this.answers = new ConcurrentDictionary<string, Answer>();
            this.audioSource = this.GetComponent<AudioSource>();

            this.timer.OnStep(2, () =>
            {
                this.ShowAnswers().CatchErrors();
            });

            this.timer.OnStep(3, () =>
            {
                this.BgMusic.Stop();
                this.ShowVotes().CatchErrors();
            });
            var bellSound = Resources.Load<AudioClip>(Constants.BellSmallMutedSoundPath);
            var cinematicBoomSound = Resources.Load<AudioClip>(Constants.CinematicBoomSoundPath);
            this.uiMenuButtonScroll01Sound = Resources.Load<AudioClip>(Constants.UiMenuButtonScroll01SoundPath);
            this.timer.OnLastSeconds(10, () => this.PanelAudio.PlayOneShot(bellSound));
            this.timer.OnLastSeconds(1, () => this.PanelAudio.PlayOneShot(cinematicBoomSound));

            this.room = ServiceProvider.Get<Room>();
            this.musicClient = ServiceProvider.Get<MusicClient>();
            this.gameOptions = ServiceProvider.Get<GameOptions>();
            this.BindToMusicClient();
        }

        // Start is called after Awake and OnEnable
        private async void Start()
        {
            this.RoundNumberPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Round {this.room.CurrentRound.Number}";

            var song = this.room.Playlist.Songs[this.room.CurrentRound.Number - 1];
            this.PlaylistScript.InitializeAsync(this.room.Playlist, song).CatchErrors();

            this.audioSource.clip = await ServiceProvider.Get<CacheService>().HandleSongAsync(song.PreviewUrl, song.PreviewHash);
            this.gameOptions.ApplyVolume();

            this.snippet = song.Snippet.Contains("{") ?
                    new Snippet(song.Snippet) : new Snippet(song.Snippet, Snippet.Mode.LastTwoWords);

            this.correctAnswer.Name = this.snippet.Answer;

            this.LoadPlayers();
            this.InstructionsListen.gameObject.SetActive(true);

            // Show Round number for at least 1 second
            if (Time.timeSinceLevelLoad < 1)
            {
                await new WaitForSeconds(1 - Time.timeSinceLevelLoad);
            }
            GameObject.Destroy(this.RoundNumberPanel);

            this.secondsToPlay = song.EndSecond.HasValue ?
               song.EndSecond.Value - song.StartSecond.GetValueOrDefault() : this.gameOptions.SongPlayLengthSeconds;
            this.audioSource.Play();

            while (this.audioSource.time <= this.secondsToPlay)
            {
                await new WaitForSeconds(0.01f);
            }

            this.audioSource.Pause();
            this.Instructions.text = this.snippet.Question;
            this.InstructionsListen.gameObject.SetActive(false);
            this.InstructionsHeader.gameObject.SetActive(true);
            this.Instructions.gameObject.SetActive(true);
            foreach (var player in this.playerScripts)
            {
                player.SetActionState(ActionState.Pending);
            }

            if (this && !this.isPaused)
            {
                await this.musicClient.AskAsync(song.Id, this.snippet.Answer);
                this.timer.StartCountdown(36);
                this.PlayBgMusic();
            }
            ////Enable this for testing
            //if (Application.platform == RuntimePlatform.WindowsEditor)
            //{
            //    ServiceProvider.Initialize();
            //    var mockProvider = new MockProvider(8);
            //    ServiceProvider.Add(mockProvider.FakeRoom);
            //    ServiceProvider.Add(mockProvider);

            //    this.room = mockProvider.FakeRoom;
            //    this.answers = mockProvider.FakeAnswers;
            //    this.snippet = new Snippet("Fake correct answer");
            //    this.correctAnswer.Name = this.snippet.Answer;
            //    this.musicClient = ServiceProvider.Get<MusicClient>();

            //    this.BindToMusicClient();
            //    this.LoadPlayers();
            //    this.Instructions.text = "I’ll tell you once more before I get off the lorem ipsum dolor sit amet before and after god said to be or not to be _____";
            //    this.timer.StartCountdown(5);

            //    this.timer.OnStep(2, () =>
            //    {
            //        this.ShowAnswers();
            //        foreach (var player in this.room.Players)
            //        {
            //            //this.answers.Shuffle();
            //            this.musicClient.AnswerVotes.Enqueue(mockProvider.GetVote(this.correctAnswer));
            //        }
            //    });

            //    this.timer.OnStep(3, () =>
            //    {
            //        this.ShowVotes().CatchErrors();
            //    });

            //    return;
            //}
        }

        private void BindToMusicClient()
        {
            this.musicClient.Disconnected.AddListener(this, error =>
            {
                Debug.LogException(error);
            });
            this.musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));
            this.musicClient.Answered.AddListener(this, answer =>
            {
                this.musicClient.RelaxAsync(answer.Player.Id).CatchErrors();

                answer.Normalize(this.correctAnswer.Name);
                this.answers.TryAdd(answer.Player.Id, answer);

                this.playerScripts.Find(p => p.Id.Equals(answer.Player.Id)).SetActionState(ActionState.Completed);
                this.PanelAudio.PlayOneShot(this.uiMenuButtonScroll01Sound);

                if (this.answers.Count == this.room.Players.Count)
                {
                    this.timer.SkipStep();
                }
            });
            this.musicClient?.VotedAnswer.AddListener(this, vote =>
            {
                this.musicClient.RelaxAsync(vote.By.Id).CatchErrors();

                var answer = this.UIAnswers.Find(a => string.Equals(a.AnswerText, vote.Item.Name,
                    StringComparison.OrdinalIgnoreCase));
                answer.AddVote(vote);
                answer.VotePlayerScripts.Add(this.playerScripts.Find(ps => string.Equals(ps.Id, vote.By.Id)));

                if (answer.IsCorrect)
                {
                    this.room.CurrentRound.Score.AddScore(vote.By, Constants.CorrectAnswerPoints);
                }
                else
                {
                    foreach (var sameAnswer in this.answers.Values
                    .Where(a => string.Equals(vote.Item.Name, a.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        this.room.CurrentRound.Score.AddScore(sameAnswer.Player, sameAnswer.IsAutoGenerated ?
                            Constants.VotePoints / 2 : Constants.VotePoints);
                    }
                }

                this.PanelAudio.PlayOneShot(this.uiMenuButtonScroll01Sound);
                this.playerScripts.Find(p => p.Id.Equals(vote.By.Id)).SetActionState(ActionState.Completed);

                if (this.UIAnswers.Select(a => a.VoteCount).Sum() == this.room.Players.Count)
                {
                    this.timer.SkipStep();
                }
            });
        }

        private void LateUpdate()
        {
            this.TimerTMP.text = this.timer.Text;
            this.timer.Tick(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                this.Pause();
            }
        }

        private void LoadPlayers()
        {
            this.playerScripts = new List<PlayerScript>();
            foreach (var player in this.room.Players)
            {
                this.playerScripts.Add(this.LoadPlayer(player));
            }
        }

        private PlayerScript LoadPlayer(Player player)
        {
            var playerScript = PlayerScript.Instantiate(this.PlayersContainer.transform, player);
            playerScript.ListenTo(audioSource, this.GetComponent<AudioPeer>());

            return playerScript;
        }

        private async Task ShowVotes()
        {
            this.AnswersContainerHeader.gameObject.SetActive(false);

            this.musicClient.RelaxAsync(this.room.Players.Select(p => p.Id).ToList()).CatchErrors();

            this.gameOptions.ApplyVolume();
            var answersToShow = this.UIAnswers.Where(a => !a.IsCorrect && a.VoteCount > 0).OrderBy(a => a.VoteCount);
            this.audioSource.UnPause();
            var playTask = this.audioSource.FadeOut();

            foreach (var uiAnswer in answersToShow)
            {
                this.ShrinkAnswersExcept(uiAnswer);
                await uiAnswer.ShowVotes();
                this.UIAnswers.Remove(uiAnswer);
                GameObject.Destroy(uiAnswer);
                this.GrowBackAnswersExcept(uiAnswer);
                await new WaitForSeconds(1);
            }

            var correctUiAnswer = this.UIAnswers.Find(a => a.IsCorrect);
            this.ShrinkAnswersExcept(correctUiAnswer);
            await correctUiAnswer.ShowVotes();
            GameObject.Destroy(correctUiAnswer);

            SceneFader.Fade("Score", Color.black, 2);
        }

        private void OnDestroy()
        {
            this.musicClient.RemoveAllListenersFrom(this);
        }

        private async Task GetMissingAsnwersAsync()
        {
            var missingPlayerIds = this.room.Players
                .Select(p => p.Id)
                .Except(this.answers.Keys);

            if (missingPlayerIds.Any())
            {
                var hubResponse = await this.musicClient.GetMissingAnswersAsync(missingPlayerIds);
                if (hubResponse.IsSuccess)
                {
                    foreach (var answer in hubResponse.GetData<IEnumerable<Answer>>())
                    {
                        answer.Normalize(this.correctAnswer.Name);
                        this.answers.TryAdd(answer.Player.Id, answer);
                    }
                }
            }
        }

        private async Task ShowAnswers()
        {
            this.musicClient.RelaxAsync(this.room.Players.Select(p => p.Id)).CatchErrors();
            await this.GetMissingAsnwersAsync();
            this.answers.TryAdd(this.correctAnswer.Player.Id, this.correctAnswer);
            var shuffledAnswers = this.answers.Values.AsEnumerable().ToList();
            shuffledAnswers.Shuffle();
            this.musicClient.StartVotingAsync(shuffledAnswers).CatchErrors();

            var distinctAnswers = shuffledAnswers.Distinct(new AnswerNameComparer()).ToList();
            this.UIAnswers = new List<AnswerScript>();
            foreach (var answer in distinctAnswers)
            {
                IEnumerable<PlayerScript> authorScripts = null;
                var sameAnswers = shuffledAnswers.Where(a => string.Equals(answer.Name, a.Name));
                if (sameAnswers.Any())
                {
                    authorScripts = this.playerScripts.Where(ps =>
                                sameAnswers.Any(a => string.Equals(ps.Id, a.Player.Id)));
                }

                var answerScript = AnswerScript.Instantiate(
                    answer,
                    this.AnswersContainer.transform,
                    authorScripts,
                    string.Equals(answer.Name, correctAnswer.Name));
                this.UIAnswers.Add(answerScript);
            }

            this.InstructionsContainer.AnimateMoveTowardsAsync(
                this.InstructionsContainer.GetTopCenterReference(120),
                5,
                5)
                .CatchErrors();
            this.InstructionsHeader.gameObject.SetActive(false);
            this.Instructions.fontSizeMax = 40;
            this.AnswersContainerHeader.gameObject.SetActive(true);

            foreach (var player in this.playerScripts)
            {
                player.SetActionState(ActionState.Pending);
            }

            this.timer.StartCountdown(46);
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
            pauseScript.ShouldRestart = this.audioSource.time < this.secondsToPlay;
            this.BgMusic.Pause();
            if (this.audioSource.isPlaying) { this.audioSource.Pause(); }
            if (this.timer.Enabled)
            {
                this.timer.Pause();
                pauseScript.OnResume = () => { this.timer.Resume(); this.isPaused = false; Time.timeScale = 1; this.BgMusic.UnPause(); };
            }
            else
            {
                pauseScript.OnResume = () => { this.isPaused = false; Time.timeScale = 1; this.BgMusic.UnPause(); };
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

        private void ShrinkAnswersExcept(AnswerScript uiAnswer)
        {
            foreach (var ua in this.UIAnswers.Except(uiAnswer))
            {
                ua.ShrinkAsync().CatchErrors();
            }
        }

        private void GrowBackAnswersExcept(AnswerScript uiAnswer)
        {
            foreach (var ua in this.UIAnswers.Except(uiAnswer))
            {
                ua.GrowBackAsync().CatchErrors();
            }
        }

        private void OnPlayerKicked(string playerId)
        {
            var foundPlayer = this.room.KickPlayer(playerId);

            if (foundPlayer)
            {
                var playerScript = this.playerScripts.Find(ps => string.Equals(playerId, ps.Id));
                this.playerScripts.Remove(playerScript);
                GameObject.Destroy(playerScript.gameObject);
                this.PanelAudio.PlayOneShot(Constants.AudioClips.GetPlayerDisconnectedSound());
            }
        }

        private void PlayBgMusic()
        {
            if (this.gameOptions.MenuMusic)
            {
                this.BgMusic.loop = true;
                this.BgMusic.clip = Constants.AudioClips.GetRandomBgMusic();
                this.BgMusic.Play();
            }
        }
    }
}