using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Extensions;
using Assets.Scripts.Mock;
using Assets.Scripts.Panels;
using Assets.Scripts.Reusable;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using SharedDomain;
using SharedDomain.Domain;
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
            room = ServiceProvider.Get<Room>();
            gameOptions = ServiceProvider.Get<GameOptions>();
            musicClient = ServiceProvider.Get<MusicClient>();

            musicClient.Message.AddListener(this, message => ToastPanelScript.Instantiate(message));

            // Mock for testing
            // ServiceProvider.Initialize();
            // var mockProvider = new MockProvider(8, 10);
            // room = mockProvider.FakeRoom;
            // gameOptions = GameOptions.Default;

            audioSource = GetComponent<AudioSource>();
            gameOptions.ApplyVolume();
            ShowScores();
        }

        private void LateUpdate()
        {
            if (EventSystem.current.currentSelectedGameObject == null && ReplayButton.isActiveAndEnabled) ReplayButton.Select();
            if (Input.GetKeyDown(KeyCode.Escape) && !isPaused)
            {
                Time.timeScale = 0;
                isPaused = true;
                audioSource.Pause();
                SimplePausePanel.Instantiate(transform, true, () =>
                {
                    audioSource.UnPause();
                    isPaused = false;
                }, () => musicClient.QuitGameAsync());
            }
        }

        private void OnDestroy()
        {
            musicClient.Message.RemoveAllListenersFrom(this);
        }

        private async void ShowScores()
        {
            var totalScores = room.ScoreBoard.Scores.Select(s => s.Value + room.CurrentRound.Score.Scores[s.Key]);
            HighestScore = totalScores.OrderByDescending(s => s).First();

            foreach (var initialScore in room.ScoreBoard.Scores)
            {
                var addedScoreValue = room.CurrentRound.Score.Scores[initialScore.Key];
                var initialScoreBricks = new KeyValuePair<int, int>(GetNumberOfBricks(initialScore.Value), initialScore.Value);
                var addedScoreBricks = new KeyValuePair<int, int>(GetNumberOfBricks(addedScoreValue), addedScoreValue);
                ScoreItemV2Script
                    .InstantiateAndShowScoreAsync(initialScore.Key, initialScoreBricks, addedScoreBricks, GroupLayout.transform)
                    .CatchErrors();
            }

            audioSource.Play();

            while (GroupLayout.GetComponentsInChildren<ScoreItemV2Script>().Any(si => !si.FinishedShowingScore))
            {
                if (isPaused) audioSource.Pause();
                //wait
                await new WaitForSeconds(0.2f);
            }

            audioSource.Stop();
            audioSource.loop = false;
            room.ScoreBoard.AddScores(room.CurrentRound.Score.Scores);

            //If we left the score scene, do nothing
            if (!this) return;

            if (room.CurrentRound.Number < gameOptions.NumberOfRounds)
            {
                await new WaitForSeconds(3);
                room.NextRound();
                LoadingSpinner.Instantiate();
                var nextScene = room.CurrentRound.Number == gameOptions.NumberOfRounds ? "SpeedRound" : "Round";
                SceneFader.Fade(nextScene, Color.black, 2);
            }
            else
            {
                var endGameResult = await musicClient.EndGameWaitAsync();
                await new WaitForSeconds(2);
                if (!endGameResult.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(endGameResult.ErrorMessage);
                    return;
                }

                ShowWinner();
            }
        }

        private async void ShowWinner()
        {
            ServiceProvider.Get<CustomEventService>().TryPushRemainingEventsAsync().CatchErrors();

            var scores = room.ScoreBoard.Scores.OrderBy(s => s.Value);
            var highestScore = scores.Last().Value;
            var winners = scores.Where(s => s.Value == highestScore);

            WinnerPanel.SetActive(true);
            WinnerPointsTMP.text = $"{highestScore} points";
            WinnerNameTMP.text = string.Join("\n", winners.Select(w => w.Key.Nick));
            if (winners.Count() > 1) WinnerTextTMP.text += "s";
            Spotlight.GetComponent<AudioSource>().Play();
            await new WaitForSeconds(2);

            WinnerContainer.GetComponent<AudioSource>().Play();
            Spotlight.GetComponentInChildren<Spinner>().IsSpinning = true;
            await WinnerContainer.GetComponent<RectTransform>().AnimateMoveTowardsAsync(Spotlight.transform.position, 4, 5);
            WellDoneBox.SetActive(true);
            // Credits
            Credits.AnimateMoveTowardsAsync(
                GameObject.FindGameObjectWithTag("PositionReference").transform.position,
                10,
                5f).CatchErrors();

            // After showing winner
            await new WaitForSeconds(2);
            foreach (var player in room.Players) PlayerScript.Instantiate(PlayerContainer.transform, player);
            Spotlight.GetComponentInChildren<Spinner>().IsSpinning = false;
            PlayerContainer.SetActive(true);
            ButtonContainer.SetActive(true);
            ReplayButton.Select();
            ReplayButton.OnSelect(new BaseEventData(EventSystem.current));

            //Review
            if (!gameOptions.HasClickedOnReview)
            {
                await new WaitForSeconds(12);
                ReviewButton.gameObject.SetActive(true);
            }
        }

        private int GetNumberOfBricks(int score) => MaxNumberOfBricks * score / (HighestScore == 0 ? 1 : HighestScore);

        public void Exit()
        {
            Application.Quit();
        }

        public async void SamePlayers()
        {
            try
            {
                this.DisableAllButtons();
                LoadingSpinner.Instantiate(transform);
                var options = ServiceProvider.Get<GameOptions>();
                var room = ServiceProvider.Get<Room>();
                var response = await musicClient.StartNewGameAsync(
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
            LoadingSpinner.Instantiate(Spotlight.transform);
            try
            {
                var result = await musicClient.KickGuestsAsync(room.Players.Select(p => p.Id));
                if (!result.IsSuccess)
                {
                    ErrorPanelScript.Instantiate(result.ErrorMessage);
                    return;
                }

                room.Players.Clear();
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
            gameOptions.HasClickedOnReview = true;
            ReviewButton.gameObject.SetActive(false);
        }
    }
}