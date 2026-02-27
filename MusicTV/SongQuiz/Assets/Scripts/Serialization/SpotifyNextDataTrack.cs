using System;
using System.Text.Json.Serialization;

namespace Assets.Scripts.Serialization
{
    public sealed record SpotifyNextDataTrack
    {
        
        [JsonPropertyName("props")] public Props Props { get; set; }

        [JsonPropertyName("page")] public string Page { get; set; }

        [JsonPropertyName("query")] public Query Query { get; set; }

        [JsonPropertyName("buildId")] public Guid BuildId { get; set; }

        [JsonPropertyName("assetPrefix")] public Uri AssetPrefix { get; set; }

        [JsonPropertyName("isFallback")] public bool IsFallback { get; set; }

        [JsonPropertyName("isExperimentalCompile")]
        public bool IsExperimentalCompile { get; set; }

        [JsonPropertyName("gssp")] public bool Gssp { get; set; }

        [JsonPropertyName("scriptLoader")] public object[] ScriptLoader { get; set; }
    }

    public sealed record Props
    {
        [JsonPropertyName("pageProps")] public PageProps PageProps { get; set; }

        [JsonPropertyName("__N_SSP")] public bool NSsp { get; set; }
    }

    public sealed record PageProps
    {
        [JsonPropertyName("state")] public State State { get; set; }

        [JsonPropertyName("config")] public Config Config { get; set; }

        [JsonPropertyName("_sentryTraceData")] public string SentryTraceData { get; set; }

        [JsonPropertyName("_sentryBaggage")] public string SentryBaggage { get; set; }
    }

    public sealed record Config
    {
        [JsonPropertyName("correlationId")] public Guid CorrelationId { get; set; }

        [JsonPropertyName("clientId")] public string ClientId { get; set; }

        [JsonPropertyName("restrictionId")] public string RestrictionId { get; set; }

        [JsonPropertyName("strings")] public Strings Strings { get; set; }

        [JsonPropertyName("locale")] public string Locale { get; set; }
    }

    public sealed record Strings
    {
        [JsonPropertyName("en")] public En En { get; set; }
    }

    public sealed record En
    {
        [JsonPropertyName("translation")] public Translation Translation { get; set; }
    }

    public sealed record Translation;

    public sealed record State
    {
        [JsonPropertyName("data")] public Data Data { get; set; }

        [JsonPropertyName("settings")] public Settings Settings { get; set; }

        [JsonPropertyName("machineState")] public MachineState MachineState { get; set; }
    }

    public sealed record Data
    {
        [JsonPropertyName("entity")] public Entity Entity { get; set; }

        [JsonPropertyName("embeded_entity_uri")]
        public string EmbededEntityUri { get; set; }

        [JsonPropertyName("defaultAudioFileObject")]
        public DefaultAudioFileObject DefaultAudioFileObject { get; set; }
    }

    public sealed record DefaultAudioFileObject
    {
        [JsonPropertyName("passthrough")] public string Passthrough { get; set; }
    }

    public sealed record Entity
    {
        [JsonPropertyName("type")] public string Type { get; set; }

        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("uri")] public string Uri { get; set; }

        [JsonPropertyName("id")] public string Id { get; set; }

        [JsonPropertyName("title")] public string Title { get; set; }

        [JsonPropertyName("artists")] public Artist[] Artists { get; set; }

        [JsonPropertyName("releaseDate")] public ReleaseDate ReleaseDate { get; set; }

        [JsonPropertyName("duration")] public long Duration { get; set; }

        [JsonPropertyName("isPlayable")] public bool IsPlayable { get; set; }

        [JsonPropertyName("playabilityReason")]
        public string PlayabilityReason { get; set; }

        [JsonPropertyName("isExplicit")] public bool IsExplicit { get; set; }

        [JsonPropertyName("isNineteenPlus")] public bool IsNineteenPlus { get; set; }

        [JsonPropertyName("audioPreview")] public AudioPreview AudioPreview { get; set; }

        [JsonPropertyName("hasVideo")] public bool HasVideo { get; set; }

        [JsonPropertyName("relatedEntityUri")] public string RelatedEntityUri { get; set; }

        [JsonPropertyName("visualIdentity")] public VisualIdentity VisualIdentity { get; set; }
    }

    public sealed record Artist
    {
        [JsonPropertyName("name")] public string Name { get; set; }

        [JsonPropertyName("uri")] public string Uri { get; set; }
    }

    public sealed record AudioPreview
    {
        [JsonPropertyName("url")] public Uri Url { get; set; }
    }

    public sealed record ReleaseDate
    {
        [JsonPropertyName("isoString")] public DateTimeOffset IsoString { get; set; }
    }

    public sealed record VisualIdentity
    {
        [JsonPropertyName("backgroundBase")] public BackgroundBase BackgroundBase { get; set; }

        [JsonPropertyName("backgroundTintedBase")]
        public BackgroundBase BackgroundTintedBase { get; set; }

        [JsonPropertyName("textBase")] public BackgroundBase TextBase { get; set; }

        [JsonPropertyName("textBrightAccent")] public BackgroundBase TextBrightAccent { get; set; }

        [JsonPropertyName("textSubdued")] public BackgroundBase TextSubdued { get; set; }

        [JsonPropertyName("image")] public Image[] Image { get; set; }
    }

    public sealed record BackgroundBase
    {
        [JsonPropertyName("alpha")] public long Alpha { get; set; }

        [JsonPropertyName("blue")] public long Blue { get; set; }

        [JsonPropertyName("green")] public long Green { get; set; }

        [JsonPropertyName("red")] public long Red { get; set; }
    }

    public sealed record Image
    {
        [JsonPropertyName("url")] public Uri Url { get; set; }

        [JsonPropertyName("maxHeight")] public long MaxHeight { get; set; }

        [JsonPropertyName("maxWidth")] public long MaxWidth { get; set; }
    }

    public sealed record MachineState
    {
        [JsonPropertyName("initialized")] public bool Initialized { get; set; }

        [JsonPropertyName("showOverflowMenu")] public bool ShowOverflowMenu { get; set; }

        [JsonPropertyName("playbackMode")] public string PlaybackMode { get; set; }

        [JsonPropertyName("currentPreviewTrackIndex")]
        public long CurrentPreviewTrackIndex { get; set; }

        [JsonPropertyName("platformSupportsEncryptedContent")]
        public bool PlatformSupportsEncryptedContent { get; set; }
    }

    public sealed record Settings
    {
        [JsonPropertyName("rtl")] public bool Rtl { get; set; }

        [JsonPropertyName("session")] public Session Session { get; set; }

        [JsonPropertyName("entityContext")] public string EntityContext { get; set; }

        [JsonPropertyName("clientId")] public string ClientId { get; set; }

        [JsonPropertyName("isMobile")] public bool IsMobile { get; set; }

        [JsonPropertyName("isSafari")] public bool IsSafari { get; set; }

        [JsonPropertyName("isIOS")] public bool IsIos { get; set; }

        [JsonPropertyName("isTablet")] public bool IsTablet { get; set; }

        [JsonPropertyName("isDarkMode")] public bool IsDarkMode { get; set; }
    }

    public sealed record Session
    {
        [JsonPropertyName("accessToken")] public string AccessToken { get; set; }

        [JsonPropertyName("accessTokenExpirationTimestampMs")]
        public long AccessTokenExpirationTimestampMs { get; set; }

        [JsonPropertyName("isAnonymous")] public bool IsAnonymous { get; set; }
    }

    public sealed record Query
    {
        [JsonPropertyName("id")] public string Id { get; set; }
    }
}