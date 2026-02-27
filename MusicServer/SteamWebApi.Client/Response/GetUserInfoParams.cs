namespace SteamWebApi.Client.Response
{
    public class GetUserInfoParams : IParams
    {
        public string State { get; set; }
        public string Country { get; set; }
        public string Currency { get; set; }
    }
}