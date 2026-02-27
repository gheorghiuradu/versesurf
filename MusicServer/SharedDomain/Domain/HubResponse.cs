using Newtonsoft.Json;
using System;

namespace SharedDomain
{
    public class HubResponse
    {
        public DateTime TimeStamp { get; set; }
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string DataJson { get; set; }

        public static HubResponse Success<T>(T data)
        {
            return new HubResponse
            {
                TimeStamp = DateTime.Now,
                IsSuccess = true,
                DataJson = JsonConvert.SerializeObject(data)
            };
        }

        public static HubResponse Success()
        {
            return new HubResponse
            {
                TimeStamp = DateTime.Now,
                IsSuccess = true,
            };
        }

        public static HubResponse Error(string message)
        {
            return new HubResponse
            {
                TimeStamp = DateTime.Now,
                IsSuccess = false,
                ErrorMessage = message
            };
        }

        public static HubResponse Forbidden()
        {
            return new HubResponse
            {
                TimeStamp = DateTime.Now,
                IsSuccess = false,
                ErrorMessage = "Forbidden"
            };
        }

        public T GetData<T>()
        {
            return JsonConvert.DeserializeObject<T>(this.DataJson);
        }
    }
}