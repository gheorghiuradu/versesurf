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
using Cysharp.Threading.Tasks;
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

        private MusicClient _musicClient;
        private GameOptions _gameOptions;

        private ConcurrentDictionary<string, Answer> _answers;
        private List<AnswerScript> _uiAnswers;
        private List<PlayerScript> _playerScripts;

        private Timer _timer;
        private Snippet _snippet;
        private Answer _correctAnswer;
        private Room _room;
        private bool _isPaused;
        private float _endSecond;

        private AudioClip _uiMenuButtonScroll01Sound;
        private AudioSource _audioSource;

        // Awake is called first
        private void Awake()
        {
            // if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor) ServiceProvider.Initialize();
            _correctAnswer = new Answer { Id = Guid.NewGuid().ToString(), Player = new Player { Id = "CORRECT" } };
            _timer = new Timer();
            _answers = new ConcurrentDictionary<string, Answer>();
            _audioSource = GetComponent<AudioSource>();

            _timer.OnStep(2, () => { ShowAnswers().CatchErrors(); });

            _timer.OnStep(3, () =>
            {
                BgMusic.Stop();
                ShowVotes().CatchErrors();
            });
            var bellSound = Resources.Load<AudioClip>(Constants.BellSmallMutedSoundPath);
            var cinematicBoomSound = Resources.Load<AudioClip>(Constants.CinematicBoomSoundPath);
            _uiMenuButtonScroll01Sound = Resources.Load<AudioClip>(Constants.UiMenuButtonScroll01SoundPath);
            _timer.OnLastSeconds(10, () => PanelAudio.PlayOneShot(bellSound));
            _timer.OnLastSeconds(1, () => PanelAudio.PlayOneShot(cinematicBoomSound));

            _room = ServiceProvider.Get<Room>();
            _musicClient = ServiceProvider.Get<MusicClient>();
            _gameOptions = ServiceProvider.Get<GameOptions>();
            BindToMusicClient();
        }

        // Start is called after Awake and OnEnable
        private async void Start()
        {
            RoundNumberPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Round {_room.CurrentRound.Number}";

            var song = _room.Playlist.Songs[_room.CurrentRound.Number - 1];
            PlaylistScript.InitializeAsync(_room.Playlist, song).CatchErrors();

            _audioSource.clip = await ServiceProvider.Get<CacheService>().HandleSongAsync(song.PreviewUrl);
            _gameOptions.ApplyVolume();

            _snippet = song.Snippet.Contains("{") ? new Snippet(song.Snippet) : new Snippet(song.Snippet, Snippet.Mode.LastTwoWords);

            _correctAnswer.Name = _snippet.Answer;

            LoadPlayers();
            InstructionsListen.gameObject.SetActive(true);

            // Show Round number for at least 1 second
            if (Time.timeSinceLevelLoad < 1) await new WaitForSeconds(1 - Time.timeSinceLevelLoad);
            Destroy(RoundNumberPanel);

            _endSecond = song.EndSecond ?? _gameOptions.SongPlayLengthSeconds;
            _endSecond += 0.19f; // Manually fix timing to be exactly the same as in editor
            if (!Mathf.Approximately(song.StartSecond.GetValueOrDefault(), _audioSource.time))
                _audioSource.time = song.StartSecond.GetValueOrDefault();
            _audioSource.Play();

            while (_audioSource.time <= _endSecond) await new WaitForSeconds(0.01f);

            _audioSource.Pause();
            Instructions.text = _snippet.Question;
            InstructionsListen.gameObject.SetActive(false);
            InstructionsHeader.gameObject.SetActive(true);
            Instructions.gameObject.SetActive(true);
            foreach (var player in _playerScripts) player.SetActionState(ActionState.Pending);

            if (this && !_isPaused)
            {
                await _musicClient.AskAsync(song.Id, _snippet.Answer);
                _timer.StartCountdown(36);
                PlayBgMusic();
            }

            //Enable this for testing
            // if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsEditor)
            // {
            //     ServiceProvider.Initialize();
            //     var mockProvider = new MockProvider(8);
            //     ServiceProvider.Add(mockProvider.FakeRoom);
            //     ServiceProvider.Add(mockProvider);
            //
            //     _room = mockProvider.FakeRoom;
            //     foreach (var mockProviderFakeAnswer in mockProvider.FakeAnswers) _answers.TryAdd(mockProviderFakeAnswer.Player.Id, mockProviderFakeAnswer);
            //     _snippet = new Snippet("Fake correct {answer}");
            //     _correctAnswer.Name = _snippet.Answer;
            //     _musicClient = ServiceProvider.Get<MusicClient>();
            //
            //     BindToMusicClient();
            //     LoadPlayers();
            //     Instructions.text = "I’ll tell you once more before I get off the lorem ipsum dolor sit amet before and after god said to be or not to be _____";
            //     _timer.StartCountdown(5);
            //
            //     _timer.OnStep(2, () =>
            //     {
            //         ShowAnswers();
            //         foreach (var player in _room.Players)
            //         {
            //             //this.answers.Shuffle();
            //             // this._musicClient.VoEnqueue(mockProvider.GetVote(this._correctAnswer));
            //         }
            //     });
            //
            //     _timer.OnStep(3, () => { ShowVotes().CatchErrors(); });
            // }
        }

        private void BindToMusicClient()
        {
            _musicClient.Disconnected.AddListener(this, error => { Debug.LogException(error); });
            _musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));
            _musicClient.Answered.AddListener(this, async answer =>
            {
                _musicClient.RelaxAsync(answer.Player.Id).CatchErrors();

                answer.Normalize(_correctAnswer.Name);
                _answers.TryAdd(answer.Player.Id, answer);

                _playerScripts.Find(p => p.Id.Equals(answer.Player.Id)).SetActionState(ActionState.Completed);
                PanelAudio.PlayOneShot(_uiMenuButtonScroll01Sound);

                if (_answers.Count == _room.Players.Count)
                {
                    while (_isPaused) await UniTask.WaitForSeconds(0.5f);

                    _timer.SkipStep();
                }
            });
            _musicClient?.VotedAnswer.AddListener(this, async vote =>
            {
                _musicClient.RelaxAsync(vote.By.Id).CatchErrors();

                var answer = _uiAnswers.Find(a => string.Equals(a.AnswerText, vote.Item.Name,
                    StringComparison.OrdinalIgnoreCase));
                answer.AddVote(vote);
                answer.VotePlayerScripts.Add(_playerScripts.Find(ps => string.Equals(ps.Id, vote.By.Id)));

                if (answer.IsCorrect)
                    _room.CurrentRound.Score.AddScore(vote.By, Constants.CorrectAnswerPoints);
                else
                    foreach (var sameAnswer in _answers.Values
                                 .Where(a => string.Equals(vote.Item.Name, a.Name, StringComparison.OrdinalIgnoreCase)))
                        _room.CurrentRound.Score.AddScore(sameAnswer.Player, sameAnswer.IsAutoGenerated ? Constants.VotePoints / 2 : Constants.VotePoints);

                PanelAudio.PlayOneShot(_uiMenuButtonScroll01Sound);
                _playerScripts.Find(p => p.Id.Equals(vote.By.Id)).SetActionState(ActionState.Completed);

                if (_uiAnswers.Select(a => a.VoteCount).Sum() == _room.Players.Count)
                {
                    while (_isPaused) await UniTask.WaitForSeconds(0.5f);
                    _timer.SkipStep();
                }
            });
        }

        private void LateUpdate()
        {
            TimerTMP.text = _timer.Text;
            _timer.Tick(Time.deltaTime);

            if (Input.GetKeyDown(KeyCode.Escape)) Pause();
        }

        private void LoadPlayers()
        {
            _playerScripts = new List<PlayerScript>();
            foreach (var player in _room.Players) _playerScripts.Add(LoadPlayer(player));
        }

        private PlayerScript LoadPlayer(Player player)
        {
            var playerScript = PlayerScript.Instantiate(PlayersContainer.transform, player);
            playerScript.ListenTo(_audioSource, GetComponent<AudioPeer>());

            return playerScript;
        }

        private async Task ShowVotes()
        {
            AnswersContainerHeader.gameObject.SetActive(false);

            _musicClient.RelaxAsync(_room.Players.Select(p => p.Id).ToList()).CatchErrors();

            _gameOptions.ApplyVolume();
            var answersToShow = _uiAnswers.Where(a => !a.IsCorrect && a.VoteCount > 0).OrderBy(a => a.VoteCount);
            _audioSource.UnPause();
            var playTask = _audioSource.FadeOut();

            foreach (var uiAnswer in answersToShow)
            {
                ShrinkAnswersExcept(uiAnswer);
                await uiAnswer.ShowVotes();
                _uiAnswers.Remove(uiAnswer);
                Destroy(uiAnswer);
                GrowBackAnswersExcept(uiAnswer);
                await new WaitForSeconds(1);
            }

            var correctUiAnswer = _uiAnswers.Find(a => a.IsCorrect);
            ShrinkAnswersExcept(correctUiAnswer);
            await correctUiAnswer.ShowVotes();
            Destroy(correctUiAnswer);

            SceneFader.Fade("Score", Color.black, 2);
        }

        private void OnDestroy()
        {
            _musicClient.RemoveAllListenersFrom(this);
        }

        private async Task GetMissingAnswersAsync()
        {
            var missingPlayerIds = _room.Players
                .Select(p => p.Id)
                .Except(_answers.Keys);

            if (missingPlayerIds.Any())
            {
                var hubResponse = await _musicClient.GetMissingAnswersAsync(missingPlayerIds);
                if (hubResponse.IsSuccess)
                    foreach (var answer in hubResponse.GetData<IEnumerable<Answer>>())
                    {
                        answer.Normalize(_correctAnswer.Name);
                        _answers.TryAdd(answer.Player.Id, answer);
                    }
            }
        }

        private async Task ShowAnswers()
        {
            _musicClient.RelaxAsync(_room.Players.Select(p => p.Id)).CatchErrors();
            await GetMissingAnswersAsync();
            _answers.TryAdd(_correctAnswer.Player.Id, _correctAnswer);
            var shuffledAnswers = _answers.Values.AsEnumerable().ToList();
            shuffledAnswers.Shuffle();
            _musicClient.StartVotingAsync(shuffledAnswers).CatchErrors();

            var distinctAnswers = shuffledAnswers.Distinct(new AnswerNameComparer()).ToList();
            _uiAnswers = new List<AnswerScript>();
            foreach (var answer in distinctAnswers)
            {
                IEnumerable<PlayerScript> authorScripts = null;
                var sameAnswers = shuffledAnswers.Where(a => string.Equals(answer.Name, a.Name));
                if (sameAnswers.Any())
                    authorScripts = _playerScripts.Where(ps =>
                        sameAnswers.Any(a => string.Equals(ps.Id, a.Player.Id)));

                var answerScript = AnswerScript.Instantiate(
                    answer,
                    AnswersContainer.transform,
                    authorScripts,
                    string.Equals(answer.Name, _correctAnswer.Name));
                _uiAnswers.Add(answerScript);
            }

            InstructionsContainer.AnimateMoveTowardsAsync(
                    InstructionsContainer.GetTopCenterReference(120),
                    5,
                    5)
                .CatchErrors();
            InstructionsHeader.gameObject.SetActive(false);
            Instructions.fontSizeMax = 40;
            AnswersContainerHeader.gameObject.SetActive(true);

            foreach (var player in _playerScripts) player.SetActionState(ActionState.Pending);

            _timer.StartCountdown(46);
        }

        private void Pause()
        {
            if (_isPaused) return;

            _isPaused = true;
            Time.timeScale = 0;
            var pauseScript = PausePanelScript.Instantiate(onPlayerKicked: OnPlayerKicked);
            pauseScript.ShouldRestart = _audioSource.time < _endSecond;
            BgMusic.Pause();
            if (_audioSource.isPlaying) _audioSource.Pause();
            if (_timer.Enabled)
            {
                _timer.Pause();
                pauseScript.OnResume = () =>
                {
                    _timer.Resume();
                    _isPaused = false;
                    Time.timeScale = 1;
                    BgMusic.UnPause();
                };
            }
            else
            {
                pauseScript.OnResume = () =>
                {
                    _isPaused = false;
                    Time.timeScale = 1;
                    BgMusic.UnPause();
                };
            }

            pauseScript.OnExitAsync = async () =>
            {
                var quitGameResult = await _musicClient.QuitGameAsync();
                if (!quitGameResult.IsSuccess) ErrorPanelScript.Instantiate(quitGameResult.ErrorMessage);
            };
        }

        private void ShrinkAnswersExcept(AnswerScript uiAnswer)
        {
            foreach (var ua in _uiAnswers.Except(uiAnswer)) ua.ShrinkAsync().CatchErrors();
        }

        private void GrowBackAnswersExcept(AnswerScript uiAnswer)
        {
            foreach (var ua in _uiAnswers.Except(uiAnswer)) ua.GrowBackAsync().CatchErrors();
        }

        private void OnPlayerKicked(string playerId)
        {
            var foundPlayer = _room.KickPlayer(playerId);

            if (foundPlayer)
            {
                var playerScript = _playerScripts.Find(ps => string.Equals(playerId, ps.Id));
                _playerScripts.Remove(playerScript);
                Destroy(playerScript.gameObject);
                PanelAudio.PlayOneShot(Constants.AudioClips.GetPlayerDisconnectedSound());
            }
        }

        private void PlayBgMusic()
        {
            if (_gameOptions.MenuMusic)
            {
                BgMusic.loop = true;
                BgMusic.clip = Constants.AudioClips.GetRandomBgMusic();
                BgMusic.Play();
            }
        }
    }
}