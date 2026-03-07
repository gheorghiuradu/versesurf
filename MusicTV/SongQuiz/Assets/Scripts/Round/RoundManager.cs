using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;
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
        private float _endSecond;

        private AudioClip uiMenuButtonScroll01Sound;
        private AudioSource audioSource;

        // Awake is called first
        private void Awake()
        {
            correctAnswer = new Answer { Id = Guid.NewGuid().ToString(), Player = new Player { Id = "CORRECT" } };
            timer = new Timer();
            answers = new ConcurrentDictionary<string, Answer>();
            audioSource = GetComponent<AudioSource>();

            timer.OnStep(2, () => { ShowAnswers().CatchErrors(); });

            timer.OnStep(3, () =>
            {
                BgMusic.Stop();
                ShowVotes().CatchErrors();
            });
            var bellSound = Resources.Load<AudioClip>(Constants.BellSmallMutedSoundPath);
            var cinematicBoomSound = Resources.Load<AudioClip>(Constants.CinematicBoomSoundPath);
            uiMenuButtonScroll01Sound = Resources.Load<AudioClip>(Constants.UiMenuButtonScroll01SoundPath);
            timer.OnLastSeconds(10, () => PanelAudio.PlayOneShot(bellSound));
            timer.OnLastSeconds(1, () => PanelAudio.PlayOneShot(cinematicBoomSound));

            room = ServiceProvider.Get<Room>();
            musicClient = ServiceProvider.Get<MusicClient>();
            gameOptions = ServiceProvider.Get<GameOptions>();
            BindToMusicClient();
        }

        // Start is called after Awake and OnEnable
        private async void Start()
        {
            RoundNumberPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Round {room.CurrentRound.Number}";

            var song = room.Playlist.Songs[room.CurrentRound.Number - 1];
            PlaylistScript.InitializeAsync(room.Playlist, song).CatchErrors();

            audioSource.clip = await ServiceProvider.Get<CacheService>().HandleSongAsync(song.PreviewUrl);
            gameOptions.ApplyVolume();

            snippet = song.Snippet.Contains("{") ? new Snippet(song.Snippet) : new Snippet(song.Snippet, Snippet.Mode.LastTwoWords);

            correctAnswer.Name = snippet.Answer;

            LoadPlayers();
            InstructionsListen.gameObject.SetActive(true);

            // Show Round number for at least 1 second
            if (Time.timeSinceLevelLoad < 1) await new WaitForSeconds(1 - Time.timeSinceLevelLoad);
            Destroy(RoundNumberPanel);

            _endSecond = song.EndSecond ?? gameOptions.SongPlayLengthSeconds;
            _endSecond += 0.19f; // Manually fix timing to be exactly the same as in editor
            if (!Mathf.Approximately(song.StartSecond.GetValueOrDefault(), audioSource.time))
                audioSource.time = song.StartSecond.GetValueOrDefault();
            audioSource.Play();

            while (audioSource.time <= _endSecond) await new WaitForSeconds(0.01f);

            audioSource.Pause();
            Instructions.text = snippet.Question;
            InstructionsListen.gameObject.SetActive(false);
            InstructionsHeader.gameObject.SetActive(true);
            Instructions.gameObject.SetActive(true);
            foreach (var player in playerScripts) player.SetActionState(ActionState.Pending);

            if (this && !isPaused)
            {
                await musicClient.AskAsync(song.Id, snippet.Answer);
                timer.StartCountdown(36);
                PlayBgMusic();
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
            musicClient.Disconnected.AddListener(this, error => { Debug.LogException(error); });
            musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));
            musicClient.Answered.AddListener(this, answer =>
            {
                musicClient.RelaxAsync(answer.Player.Id).CatchErrors();

                answer.Normalize(correctAnswer.Name);
                answers.TryAdd(answer.Player.Id, answer);

                playerScripts.Find(p => p.Id.Equals(answer.Player.Id)).SetActionState(ActionState.Completed);
                PanelAudio.PlayOneShot(uiMenuButtonScroll01Sound);

                if (answers.Count == room.Players.Count) timer.SkipStep();
            });
            musicClient?.VotedAnswer.AddListener(this, vote =>
            {
                musicClient.RelaxAsync(vote.By.Id).CatchErrors();

                var answer = UIAnswers.Find(a => string.Equals(a.AnswerText, vote.Item.Name,
                    StringComparison.OrdinalIgnoreCase));
                answer.AddVote(vote);
                answer.VotePlayerScripts.Add(playerScripts.Find(ps => string.Equals(ps.Id, vote.By.Id)));

                if (answer.IsCorrect)
                    room.CurrentRound.Score.AddScore(vote.By, Constants.CorrectAnswerPoints);
                else
                    foreach (var sameAnswer in answers.Values
                                 .Where(a => string.Equals(vote.Item.Name, a.Name, StringComparison.OrdinalIgnoreCase)))
                        room.CurrentRound.Score.AddScore(sameAnswer.Player, sameAnswer.IsAutoGenerated ? Constants.VotePoints / 2 : Constants.VotePoints);

                PanelAudio.PlayOneShot(uiMenuButtonScroll01Sound);
                playerScripts.Find(p => p.Id.Equals(vote.By.Id)).SetActionState(ActionState.Completed);

                if (UIAnswers.Select(a => a.VoteCount).Sum() == room.Players.Count) timer.SkipStep();
            });
        }

        private void LateUpdate()
        {
            TimerTMP.text = timer.Text;
            timer.Tick(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape)) Pause();
        }

        private void LoadPlayers()
        {
            playerScripts = new List<PlayerScript>();
            foreach (var player in room.Players) playerScripts.Add(LoadPlayer(player));
        }

        private PlayerScript LoadPlayer(Player player)
        {
            var playerScript = PlayerScript.Instantiate(PlayersContainer.transform, player);
            playerScript.ListenTo(audioSource, GetComponent<AudioPeer>());

            return playerScript;
        }

        private async Task ShowVotes()
        {
            AnswersContainerHeader.gameObject.SetActive(false);

            musicClient.RelaxAsync(room.Players.Select(p => p.Id).ToList()).CatchErrors();

            gameOptions.ApplyVolume();
            var answersToShow = UIAnswers.Where(a => !a.IsCorrect && a.VoteCount > 0).OrderBy(a => a.VoteCount);
            audioSource.UnPause();
            var playTask = audioSource.FadeOut();

            foreach (var uiAnswer in answersToShow)
            {
                ShrinkAnswersExcept(uiAnswer);
                await uiAnswer.ShowVotes();
                UIAnswers.Remove(uiAnswer);
                Destroy(uiAnswer);
                GrowBackAnswersExcept(uiAnswer);
                await new WaitForSeconds(1);
            }

            var correctUiAnswer = UIAnswers.Find(a => a.IsCorrect);
            ShrinkAnswersExcept(correctUiAnswer);
            await correctUiAnswer.ShowVotes();
            Destroy(correctUiAnswer);

            SceneFader.Fade("Score", Color.black, 2);
        }

        private void OnDestroy()
        {
            musicClient.RemoveAllListenersFrom(this);
        }

        private async Task GetMissingAsnwersAsync()
        {
            var missingPlayerIds = room.Players
                .Select(p => p.Id)
                .Except(answers.Keys);

            if (missingPlayerIds.Any())
            {
                var hubResponse = await musicClient.GetMissingAnswersAsync(missingPlayerIds);
                if (hubResponse.IsSuccess)
                    foreach (var answer in hubResponse.GetData<IEnumerable<Answer>>())
                    {
                        answer.Normalize(correctAnswer.Name);
                        answers.TryAdd(answer.Player.Id, answer);
                    }
            }
        }

        private async Task ShowAnswers()
        {
            musicClient.RelaxAsync(room.Players.Select(p => p.Id)).CatchErrors();
            await GetMissingAsnwersAsync();
            answers.TryAdd(correctAnswer.Player.Id, correctAnswer);
            var shuffledAnswers = answers.Values.AsEnumerable().ToList();
            shuffledAnswers.Shuffle();
            musicClient.StartVotingAsync(shuffledAnswers).CatchErrors();

            var distinctAnswers = shuffledAnswers.Distinct(new AnswerNameComparer()).ToList();
            UIAnswers = new List<AnswerScript>();
            foreach (var answer in distinctAnswers)
            {
                IEnumerable<PlayerScript> authorScripts = null;
                var sameAnswers = shuffledAnswers.Where(a => string.Equals(answer.Name, a.Name));
                if (sameAnswers.Any())
                    authorScripts = playerScripts.Where(ps =>
                        sameAnswers.Any(a => string.Equals(ps.Id, a.Player.Id)));

                var answerScript = AnswerScript.Instantiate(
                    answer,
                    AnswersContainer.transform,
                    authorScripts,
                    string.Equals(answer.Name, correctAnswer.Name));
                UIAnswers.Add(answerScript);
            }

            InstructionsContainer.AnimateMoveTowardsAsync(
                    InstructionsContainer.GetTopCenterReference(120),
                    5,
                    5)
                .CatchErrors();
            InstructionsHeader.gameObject.SetActive(false);
            Instructions.fontSizeMax = 40;
            AnswersContainerHeader.gameObject.SetActive(true);

            foreach (var player in playerScripts) player.SetActionState(ActionState.Pending);

            timer.StartCountdown(46);
        }

        private void Pause()
        {
            if (isPaused) return;

            isPaused = true;
            Time.timeScale = 0;
            var pauseScript = PausePanelScript.Instantiate(onPlayerKicked: OnPlayerKicked);
            pauseScript.ShouldRestart = audioSource.time < _endSecond;
            BgMusic.Pause();
            if (audioSource.isPlaying) audioSource.Pause();
            if (timer.Enabled)
            {
                timer.Pause();
                pauseScript.OnResume = () =>
                {
                    timer.Resume();
                    isPaused = false;
                    Time.timeScale = 1;
                    BgMusic.UnPause();
                };
            }
            else
            {
                pauseScript.OnResume = () =>
                {
                    isPaused = false;
                    Time.timeScale = 1;
                    BgMusic.UnPause();
                };
            }

            pauseScript.OnExitAsync = async () =>
            {
                var quitGameResult = await musicClient.QuitGameAsync();
                if (!quitGameResult.IsSuccess) ErrorPanelScript.Instantiate(quitGameResult.ErrorMessage);
            };
        }

        private void ShrinkAnswersExcept(AnswerScript uiAnswer)
        {
            foreach (var ua in UIAnswers.Except(uiAnswer)) ua.ShrinkAsync().CatchErrors();
        }

        private void GrowBackAnswersExcept(AnswerScript uiAnswer)
        {
            foreach (var ua in UIAnswers.Except(uiAnswer)) ua.GrowBackAsync().CatchErrors();
        }

        private void OnPlayerKicked(string playerId)
        {
            var foundPlayer = room.KickPlayer(playerId);

            if (foundPlayer)
            {
                var playerScript = playerScripts.Find(ps => string.Equals(playerId, ps.Id));
                playerScripts.Remove(playerScript);
                Destroy(playerScript.gameObject);
                PanelAudio.PlayOneShot(Constants.AudioClips.GetPlayerDisconnectedSound());
            }
        }

        private void PlayBgMusic()
        {
            if (gameOptions.MenuMusic)
            {
                BgMusic.loop = true;
                BgMusic.clip = Constants.AudioClips.GetRandomBgMusic();
                BgMusic.Play();
            }
        }
    }
}