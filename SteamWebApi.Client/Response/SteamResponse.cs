namespace SteamWebApi.Client.Response
{
    public class SteamResponse<T> where T : IParams
    {
        public BaseResponse Response { get; set; }
        public BaseError Error { get; set; }

        public class BaseResponse
        {
            public string Result { get; set; }
            public T Params { get; set; }
        }

        public class BaseError
        {
            public int ErrorCode { get; set; }
            public string ErrorDesc { get; set; }
        }
    }

    public interface IParams { }
}