using System;
using System.Threading.Tasks;
using Assets.Scripts.Panels;
using Assets.Scripts.Serialization;
using Assets.Scripts.Services;
using Assets.Scripts.VIP;
using SharedDomain;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Assets.Scripts.Splash
{
    public class SplashManager : MonoBehaviour
    {
        private async void Start()
        {
            try
            {
                Application.targetFrameRate = 60;
                UnityMainThreadDispatcher.Initialize();
                ServiceProvider.Initialize();
#if UNITY_STANDALONE
                new GameObject(nameof(SteamManager)).AddComponent<SteamManager>();
#endif
                var musicClient = ServiceProvider.Get<MusicClient>();
                var waiter = new Waiter(5);

                var success = await waiter.WithRetryAsync(async () =>
                {
#if UNITY_STANDALONE
                    while (!SteamManager.Initialized) await new WaitForSeconds(1);
#endif
                    await LocalizationSettings.InitializationOperation;
                    var gameOptions = ServiceProvider.Get<GameOptions>();
                    await musicClient.ConnectAsync();
                    var locale = LocalizationSettings.SelectedLocale;
                    var localeId = locale?.Formatter?.ToString() ?? "en";
                    await BookRoomAsync(Guid.NewGuid().ToString(), localeId);
                    gameOptions.ApplySelectedResolution();
                    gameOptions.ApplyVolume();
                    VipManager.Initialize(musicClient);
                }, 5);

                if (!success) throw new Exception("Failed to initialize game");

                //await LocalizationSettings.InitializationOperation;
                SceneFader.Fade("MainMenu", Color.black, 2);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
                var errorPanel = ErrorPanelScript.Instantiate("There was a problem initializing the game and communicating with the servers. Please try again later, or check your connection.");
                errorPanel.OnOkPressed.AddListener(() => Application.Quit());
            }
        }

        private async Task BookRoomAsync(string playFabId, string localeId)
        {
            // Book room
            var appSettings = ServiceProvider.Get<AppSettings>();
            var musicClient = ServiceProvider.Get<MusicClient>();

            var request = new BookRoomRequest(
                appSettings.AvailableCharacters,
                appSettings.AvailableColorCodes,
                Application.version,
                Application.platform.ToString(),
                playFabId,
                localeId);

            var roomResponse = await musicClient.BookRoomAsync(request);

            if (!roomResponse.IsSuccess)
            {
                ErrorPanelScript.Instantiate(roomResponse.ErrorMessage);
                throw new Exception(roomResponse.ErrorMessage);
            }

            var room = new Room
            {
                Code = roomResponse.GetData<string>(),
                RoomRequest = request
            };
            musicClient.BindEventsToRoom(room);
            ServiceProvider.AddOrReplace(room);
        }

#if UNITY_ANDROID
        public async void OnSignIn()
        {
        }
#endif
    }
}