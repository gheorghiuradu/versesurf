using Assets.Scripts.Extensions;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;

#if UNITY_STANDALONE

using Steamworks;

#endif

using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.Score
{
    public class ScoreManager : UnityEventUser
    {
        public GameObject GroupLayout;
        public GameObject WinnerPanel;
        public GameObject WinnerContainer;
        public TextMeshProUGUI WinnerNameTMP;
        public TextMeshProUGUI WinnerPointsTMP;
        public TextMeshProUGUI WinnerTextTMP;
        public GameObject WellDoneBox;
        public GameObject Spotlight;
        public GameObject ButtonContainer;
        public Button ReplayButton;
        public GameObject PlayerContainer;
        public RectTransform Credits;
        public Button ReviewButton;

        private AudioSource audioSource;

        private Room room;
        private GameOptions gameOptions;
        private MusicClient musicClient;

        private const int MaxNumberOfBricks = 14;
        private int HighestScore;
        private bool isPaused;

        private void Start()
        {
            this.room = ServiceProvider.Get<Room>();
            this.gameOptions = ServiceProvider.Get<GameOptions>();
            this.musicClient = ServiceProvider.Get<MusicClient>();

            this.musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));

            // Mock for testing
            //ServiceProvider.Initialize();
            //var mockProvider = new MockProvider(players: 8, currentRound: 10);
            //this.room = mockProvider.FakeRoom;
            //this.gameOptions = GameOptions.Default;

            this.audioSource = this.GetComponent<AudioSource>();
            this.gameOptions.ApplyVolume();
            this.ShowScores();
        }

        private void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null && this.ReplayButton.isActiveAndEnabled)
            {
                this.ReplayButton.Select();
            }
            if (Input.GetKeyDown(KeyCode.Escape) && !this.isPaused)
            {
                Time.timeScale = 0;
                this.isPaused = true;
                this.audioSource.Pause();
                SimplePausePanel.Instantiate(this.transform, true, () =>
                {
                    this.audioSource.UnPause();
                    this.isPaused = false;
                }, () => this.musicClient.QuitGameAsync());
            }
        }

        private void OnDestroy()
        {
            this.musicClient.Message.RemoveAllListenersFrom(this);
        }

        private async void ShowScores()
        {
            //TODO: kick player
            var totalScores = this.room.ScoreBoard.Scores.Select(s => s.Value + this.room.CurrentRound.Score.Scores[s.Key]);
            this.HighestScore = totalScores.OrderByDescending(s => s).First();

            foreach (var initialScore in this.room.ScoreBoard.Scores)
            {
                var addedScoreValue = this.room.CurrentRound.Score.Scores[initialScore.Key];
                var initalScoreBricks = new KeyValuePair<int, int>(this.GetNumberOfBricks(initialScore.Value), initialScore.Value);
                var addedScoreBricks = new KeyValuePair<int, int>(this.GetNumberOfBricks(addedScoreValue), addedScoreValue);
                ScoreItemV2Script
                    .InstantiateAndShowScoreAsync(initialScore.Key, initalScoreBricks, addedScoreBricks, this.GroupLayout.transform)
                    .CatchErrors();
            }
            this.audioSource.Play();

            while (this.GroupLayout.GetComponentsInChildren<ScoreItemV2Script>().Any(si => !si.FinishedShowingScore))
            {
                if (this.isPaused)
                {
                    this.audioSource.Pause();
                }
                //wait
                await new WaitForSeconds(0.2f);
            }
            this.audioSource.Stop();
            this.audioSource.loop = false;
            //this.audioSource.PlayOneShot(Constants.AudioClips.GetCartoonComputerSound01());
            this.room.ScoreBoard.AddScores(this.room.CurrentRound.Score.Scores);

            //If we left the score scene, do nothing
            if (!this)
            {
                return;
            }

            if (this.room.CurrentRound.Number < this.gameOptions.NumberOfRounds)
            {
                await new WaitForSeconds(3);
                this.room.NextRound();
                LoadingSpinner.Instantiate();
                var nextScene = this.room.CurrentRound.Number == this.gameOptions.NumberOfRounds ?
                    "SpeedRound" : "Round";
                SceneFader.Fade(nextScene, Color.black, 2);
            }
            else
            {
                var endGameResult = await this.musicClient.EndGameWaitAsync();
                await new WaitForSeconds(2);
                if (!endGameResult.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(endGameResult.ErrorMessage);
                    return;
                }
                this.ShowWinner();
            }
        }

        private async void ShowWinner()
        {
            ServiceProvider.Get<CustomEventService>().TryPushRemainingEventsAsync().CatchErrors();

            var scores = this.room.ScoreBoard.Scores.OrderBy(s => s.Value);
            var highestScore = scores.Last().Value;
            var winners = scores.Where(s => s.Value == highestScore);

            this.WinnerPanel.SetActive(true);
            this.WinnerPointsTMP.text = $"{highestScore} points";
            this.WinnerNameTMP.text = string.Join("\n", winners.Select(w => w.Key.Nick));
            if (winners.Count() > 1)
            {
                this.WinnerTextTMP.text += "s";
            }
            this.Spotlight.GetComponent<AudioSource>().Play();
            await new WaitForSeconds(2);

            this.WinnerContainer.GetComponent<AudioSource>().Play();
            this.Spotlight.GetComponentInChildren<Spinner>().IsSpinning = true;
            await this.WinnerContainer.GetComponent<RectTransform>().AnimateMoveTowardsAsync(this.Spotlight.transform.position, 4, 5);
            this.WellDoneBox.SetActive(true);
            // Credits
            this.Credits.AnimateMoveTowardsAsync(
                GameObject.FindGameObjectWithTag("PositionReference").transform.position,
                10,
                5f).CatchErrors();

            // After showing winner
            await new WaitForSeconds(2);
            foreach (var player in this.room.Players)
            {
                PlayerScript.Instantiate(this.PlayerContainer.transform, player);
            }
            this.Spotlight.GetComponentInChildren<Spinner>().IsSpinning = false;
            this.PlayerContainer.SetActive(true);
            this.ButtonContainer.SetActive(true);
            this.ReplayButton.Select();
            this.ReplayButton.OnSelect(new BaseEventData(EventSystem.current));

            //Review
            if (!this.gameOptions.HasClickedOnReview)
            {
                await new WaitForSeconds(12);
                this.ReviewButton.gameObject.SetActive(true);
            }
        }

        private int GetNumberOfBricks(int score)
        {
            return MaxNumberOfBricks * score / (this.HighestScore == 0 ? 1 : this.HighestScore);
        }

        public void Exit()
        {
            Application.Quit();
        }

        public async void SamePlayers()
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

        public async void MainMenu()
        {
            LoadingSpinner.Instantiate(this.Spotlight.transform);
            try
            {
                var result = await this.musicClient.KickGuestsAsync(this.room.Players.Select(p => p.Id));
                if (!result.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(result.ErrorMessage);
                    return;
                }

                this.room.Players.Clear();
                SceneManager.LoadScene("MainMenu");
            }
            catch (System.Exception)
            {
                LoadingSpinner.Destroy();
                ErrorPanelScript.Instantiate("There was an error on the server.\nPlease try again.");
            }
        }

        public void OnClickReview()
        {
            this.gameOptions.HasClickedOnReview = true;
            ServiceProvider.Get<PlayFabService>().SaveGameOptionsAsync(this.gameOptions).CatchErrors();
            this.ReviewButton.gameObject.SetActive(false);
#if UNITY_STANDALONE
            SteamFriends.ActivateGameOverlayToWebPage("https://store.steampowered.com/app/1315390/Verse_Surf/#review_create");
#endif
        }
    }
}