using SharedDomain.Messages.Queries;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Settings;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Assets.Scripts.Serialization
{
    public class GameOptions
    {
        public const int MaxSongPlayLengthSeconds = 20;
        public const int MinSongPlayLengthSeconds = 1;
        public const int MaxNumberOfRounds = 10;
        public const int MinNumberOfRounds = 1;
        public const int MinVolume = 0;
        public const int MaxVolume = 100;

        private const int DefaultSongLength = 6;
        private const int DefaultNumberOfRounds = 5;
        private const bool DefaultAllowExplicit = false;
        private const int DefaultVolume = 70;
        private const bool DefaultFullScreen = true;
        private const bool DefaultMenuMusic = true;

        [System.Obsolete("Defaulting to 6, soon all playlists will be featured.")]
        public int SongPlayLengthSeconds => DefaultSongLength;

        public int NumberOfRounds { get; set; } = DefaultNumberOfRounds;
        public bool AllowExplicit { get; set; } = DefaultAllowExplicit;
        public int Volume { get; set; } = DefaultVolume;
        public bool FullScreen { get; set; } = DefaultFullScreen;
        public int ResolutionWidth { get; set; } = Screen.currentResolution.width;
        public int ResolutionHeigth { get; set; } = Screen.currentResolution.height;
        public bool MenuMusic { get; set; } = DefaultMenuMusic;
        public bool HasClickedOnReview { get; set; }

        [JsonIgnore]
        public PlaylistOptions PlaylistOptions => new()
        {
            AllowExplicit = this.AllowExplicit,
            Language = (LocalizationSettings.SelectedLocale?.Identifier.CultureInfo.TwoLetterISOLanguageName),
            NumberOfSongs = this.NumberOfRounds
        };

        public static GameOptions Default => new()
        {
            NumberOfRounds = DefaultNumberOfRounds,
            AllowExplicit = DefaultAllowExplicit,
            Volume = DefaultVolume,
            FullScreen = DefaultFullScreen,
            ResolutionWidth = Screen.currentResolution.width,
            ResolutionHeigth = Screen.currentResolution.height,
            MenuMusic = DefaultMenuMusic
        };

        public Dictionary<string, string> ToDictionary()
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(
                JsonSerializer.Serialize(this));
        }

        public void ApplyVolume()
        {
            foreach (var audio in Resources.FindObjectsOfTypeAll<AudioSource>())
            {
                audio.volume = this.Volume / 100f;
            }
        }

        public void ApplyVolume(GameObject @object)
        {
            var audioS = @object.GetComponent<AudioSource>();
            if (audioS != null)
            {
                audioS.volume = this.Volume / 100f;
            }
            foreach (var audio in @object.GetComponentsInChildren<AudioSource>(true))
            {
                audio.volume = this.Volume / 100f;
            }
        }

        public void SetResolution(Resolution resolution)
        {
            this.ResolutionHeigth = resolution.height;
            this.ResolutionWidth = resolution.width;
        }

        public void ApplySelectedResolution()
        {
            Screen.SetResolution(this.ResolutionWidth, this.ResolutionHeigth, this.FullScreen);
        }

        private static string SavePath => System.IO.Path.Combine(Application.persistentDataPath, "gameOptions.json");

        public void Save()
        {
            var json = JsonSerializer.Serialize(this);
            System.IO.File.WriteAllText(SavePath, json);
        }

        public static GameOptions Load()
        {
            if (!System.IO.File.Exists(SavePath))
                return Default;

            try
            {
                var json = System.IO.File.ReadAllText(SavePath);
                return JsonSerializer.Deserialize<GameOptions>(json) ?? Default;
            }
            catch
            {
                return Default;
            }
        }
    }
}