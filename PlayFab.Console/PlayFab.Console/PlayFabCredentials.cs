namespace PlayFab.Console
{
    public sealed class PlayFabCredentials
    {
        public string TitleId { get; set; }
        public string DeveloperSecretKey { get; set; }

        public PlayFabApiSettings ToPlayFabApiSettings() =>
            new()
            {
                TitleId = TitleId,
                DeveloperSecretKey = DeveloperSecretKey
            };
    }
}