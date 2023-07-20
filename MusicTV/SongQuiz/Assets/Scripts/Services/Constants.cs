using System.IO;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public static class Constants
    {
        public const int SteamAppId = 1315390;
        public const int VotePoints = 100;
        public const int CorrectAnswerPoints = 300;
        public const int SpeedAnswerPointsMultiplier = 10;
        public const string ReconnectingPanelPrefabPath = "Prefabs/ReconnectingPanel";
        public const string CharacterPath = "Sprites/characters";

        public const string BellSmallMutedSoundPath = "Audio/bell_small_muted_02";
        public const string CinematicBoomSoundPath = "Audio/cinematic_deep_boom_impact_01";
        public const string WhooshSlowDeep09SoundPath = "Audio/whoosh_slow_deep_09";
        public const string UiMenuButtonScroll02SoundPath = "Audio/ui_menu_button_scroll_02";
        public const string UiMenuButtonScroll01SoundPath = "Audio/ui_menu_button_scroll_01";
        public const string UiMenuButtonClick03SoundPath = "Audio/ui_menu_button_click_03";

        public const string TextureOrangePath = "Sprites/texture_orange";
        public const string ScoreItemPrefabPath = "Prefabs/ScoreItem";
        public const string SteamManagerPrefabPath = "Prefabs/SteamManager";
        public const string StoreProductButtonPrefabPath = "Prefabs/StoreProductButton";
        public const string StorePrefabPath = "Prefabs/Store";
        public const string InventoryProductButtonPrefabPath = "Prefabs/InventoryProductButton";
        public const string OptionsPanelPrefabPath = "Prefabs/OptionsPanel";

        public static string SongCacheFullPath => Path.Combine(Application.temporaryCachePath, "Songs");

        public static string PlaylistImageCacheFullPath => Path.Combine(Application.temporaryCachePath, "PlaylistImg");

        public static class AudioClips
        {
            public static AudioClip GetCashRegisterSound() =>
                Resources.Load<AudioClip>("Audio/cash_register_open_coins_cha_ching_01");

            public static AudioClip GetCartoonComputerSound01() =>
                Resources.Load<AudioClip>("Audio/cartoon_electronic_computer_code_01");

            public static AudioClip GetPlayerDisconnectedSound() =>
                Resources.Load<AudioClip>("Audio/jingle_chime_16_negative");

            public static AudioClip GetPlayerConnectedSound() =>
                Resources.Load<AudioClip>("Audio/jingle_chime_01_positive");

            public static AudioClip GetPointsTicker01Sound() =>
                Resources.Load<AudioClip>("Audio/points_ticker_bonus_no_score_01");

            public static AudioClip GetVipActivatedSound() =>
                Resources.Load<AudioClip>("Audio/jingle_chime_07_positive");

            public static AudioClip GetVipDiscardedSound() =>
                Resources.Load<AudioClip>("Audio/jingle_chime_24_negative");

            public static AudioClip GetPointsClickSound() =>
                Resources.Load<AudioClip>("Audio/ui_menu_button_click_05");

            public static AudioClip GetSpeedAnswerInSound() =>
                Resources.Load<AudioClip>("Audio/sci-fi_vehicle_thrusters_fail_01");

            public static AudioClip GetMetalMoveSound() =>
                Resources.Load<AudioClip>("Audio/metal_on_wood_rolling_ball_loop_01");

            public static AudioClip GetRandomMenuMusic() =>
                Random.Range(0, 4) switch
                {
                    0 => Resources.Load<AudioClip>("Audio/music_fun_funky_gnome"),
                    1 => Resources.Load<AudioClip>("Audio/music_fun_funky_mushroom"),
                    2 => Resources.Load<AudioClip>("Audio/music_fun_funky_whistle_groove_loop"),
                    3 => Resources.Load<AudioClip>("Audio/theme/theme"),
                    _ => Resources.Load<AudioClip>("Audio/music_fun_funky_gnome"),
                };

            public static AudioClip GetRandomBgMusic() =>
                Random.Range(0, 4) switch
                {
                    0 => Resources.Load<AudioClip>("Audio/theme/Break_Trumpet_No_Drums"),
                    1 => Resources.Load<AudioClip>("Audio/theme/Chill_Lounge&Trumpet_No_Drums"),
                    2 => Resources.Load<AudioClip>("Audio/theme/Chill_Lounge_No_Drums"),
                    3 => Resources.Load<AudioClip>("Audio/theme/Chill_Lounge_Drums"),
                    _ => Resources.Load<AudioClip>("Audio/theme/Chill_Lounge_Drums")
                };
        }

        public static class Colors
        {
            public static Color32 Accent1 => GetHtmlColor("#D9947B");
            public static Color32 CorrectAnswerBorder => GetHtmlColor("#547370");
            public static Color32 CorrectAnswerBackground => GetHtmlColor("#D9DAD4");
            public static Color32 CorrectAnswerText => GetHtmlColor("#264047");
            public static Color32 InitialScoreBrick => GetHtmlColor("#AB5E50");
            public static Color32 NewScoreBrick => GetHtmlColor("#8E4942");
            public static Color32 BlackText => GetHtmlColor("#2D2D2D");
            public static Color32 Accent2 => GetHtmlColor("#C77F6A");

            public static Color32 GetHtmlColor(string hex) =>
                ColorUtility.TryParseHtmlString(hex, out var result) ? result : Color.white;
        }

        public static class Animations
        {
            public const string MoveTurntableArm = nameof(MoveTurntableArm);
            public const string MoveTurntableArmBack = nameof(MoveTurntableArmBack);
            public const string CharacterJoin = nameof(CharacterJoin);
            public const string CharacterDance = nameof(CharacterDance);
        }

        public static class Materials
        {
            public static Material GetGrayScale() => Resources.Load<Material>("Materials/GrayScale");
        }
    }
}