namespace SteamWebApi.Client.Response
{
    public class InitTxnParams : IParams
    {
        public int OrderId { get; set; }
        public int TransId { get; set; }
    }
}