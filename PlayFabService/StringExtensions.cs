using Newtonsoft.Json;
using System.Collections.Generic;

namespace PlayFabService
{
    public static class StringExtensions
    {
        public static Dictionary<string, string> ToDictionary(this string text)
        {
            return string.IsNullOrWhiteSpace(text) ?
                new Dictionary<string, string>() :
                JsonConvert.DeserializeObject<Dictionary<string, string>>(text);
        }
    }
}