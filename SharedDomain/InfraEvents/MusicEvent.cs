using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SharedDomain.InfraEvents
{
    public class MusicEvent
    {
        public string Sender { get; set; }
        public DateTime TimeStamp { get; set; }
        public string EventType { get; set; }
        public string PayloadJson { get; set; }
        public string PayloadName { get; set; }
        public string PayloadType { get; set; }

        // Empty constructor for serialization
        public MusicEvent()
        {
        }

        public MusicEvent(EventType type)
        {
            this.Sender = Assembly.GetEntryAssembly().FullName;
            this.TimeStamp = DateTime.UtcNow;
            this.EventType = type.ToString();
        }

        public MusicEvent(EventType type, object payload)
        {
            this.Sender = Assembly.GetEntryAssembly().FullName;
            this.TimeStamp = DateTime.UtcNow;
            this.EventType = type.ToString();
            this.UpdatePayload(payload);
        }

        public MusicEvent(EventType type, string payload)
        {
            this.Sender = Assembly.GetEntryAssembly().FullName;
            this.TimeStamp = DateTime.UtcNow;
            this.EventType = type.ToString();
            this.PayloadJson = payload;
            this.PayloadName = nameof(payload);
            this.PayloadType = payload.GetType().Name;
        }

        public T GetPayloadAs<T>() => JsonConvert.DeserializeObject<T>(this.PayloadJson);

        public Dictionary<string, string> GetPayloadAsFlatDictionary()
        {
            var jobject = JObject.Parse(this.PayloadJson);

            return jobject.Descendants()
                .Where(j => !j.Children().Any())
                .Aggregate(
                    new Dictionary<string, string>(),
                    (props, jtoken) =>
            {
                props.Add(jtoken.Path, jtoken.ToString());
                return props;
            });
        }

        public void UpdatePayload(object payload)
        {
            this.PayloadJson = JsonConvert.SerializeObject(payload);
            this.PayloadName = nameof(payload);
            this.PayloadType = payload.GetType().Name;
        }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum EventType
    {
        AddedSong,
        AddedPlaylist,
        CreatedRoom,
        VotedPlaylist,
        PlayedSong,
        GameStarted,
        GameEnded,
        PlayerJoined,
        PurgedRoom,
        StartedActivateItem,
        FinishedActivateItem,
        StartConsumeItem,
        FinishedConsumeItem,
        StartEnsureFreeItemsPolicy,
        FinishEnsureFreeItemsPolicy,
        RemovedRoom,
        HostDisconnected,
        NotificationRequestUnauthorized,
        NotificationRequestAuthorized,
        GameQuit,
        Custom,
        PurchasedItem
    }
}