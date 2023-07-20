using Assets.Scripts.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SharedDomain;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Scripts.Services
{
    public static class ServiceProvider
    {
        public static bool IsInitialized;
        private static List<object> ServiceCollection { get; } = new List<object>();

        public static T Get<T>() where T : class
        {
            if (!IsInitialized) throw new System.Exception("ServiceCollection not initialized");

            return ServiceCollection.Find(s => s is T) as T;
        }

        public static void Initialize()
        {
            if (IsInitialized) return;

            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Converters = new List<JsonConverter> { new StringEnumConverter() }
            };

            var path = Path.Combine(Application.streamingAssetsPath, "appsettings.json");
#if UNITY_STANDALONE || UNITY_IOS
            var json = File.ReadAllText(path);
#elif UNITY_ANDROID
            var request = UnityWebRequest.Get(path);
            request.SendWebRequest();
            while (!request.isDone)
            {
                //wait
            }
            var json = request.downloadHandler.text;
#endif
            var settings = JsonConvert.DeserializeObject<AppSettings>(json);
            ServiceCollection.Add(settings);

            var client = new MusicClient(settings.HubUrl, settings.ApiUrl);
            var playfabService = new PlayFabService();
            ServiceCollection.Add(client);
            ServiceCollection.Add(new CacheService(client));
            ServiceCollection.Add(playfabService);
            ServiceCollection.Add(new CustomEventService(playfabService, client));

            IsInitialized = true;

            Application.quitting += () =>
            {
                if (!(Get<Room>() is null))
                {
                    client.RemoveRoomAsync().Wait();
                }
                client.DisconnectAsync();
            };
        }

        public static void Add(object obj)
        {
            ServiceCollection.Add(obj);
        }

        public static void AddOrReplace<T>(T obj) where T : class
        {
            var existing = ServiceCollection.Find(o => o is T);
            if (!(existing is null))
            {
                ServiceCollection.Remove(existing);
            }
            ServiceCollection.Add(obj);
        }
    }
}