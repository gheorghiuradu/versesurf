using Newtonsoft.Json;

namespace TaskService.Extensions
{
    public static class StringExtensions
    {
        public static T ConvertTo<T>(this string json) => JsonConvert.DeserializeObject<T>(json);
    }
}