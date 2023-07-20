using Newtonsoft.Json;

namespace MusicServer.Extensions
{
    public static class ObjectExtensions
    {
        public static TResult ConvertTo<TResult>(this object @object)
        {
            return JsonConvert.DeserializeObject<TResult>(
                JsonConvert.SerializeObject(@object)
                );
        }
    }
}