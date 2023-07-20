using Newtonsoft.Json;

namespace MusicApi.Serverless.Extensions
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