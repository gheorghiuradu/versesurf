using MusicEventDbApi;
using SharedDomain.InfraEvents;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MusicServer.Services
{
    public class MusicEventService
    {
        private readonly MusicEventDbClient dbClient;

        public MusicEventService(MusicEventDbClient dbClient)
        {
            this.dbClient = dbClient;
        }

        public Task PostEventAsync(EventType eventType)
        {
            var dbEvent = new MusicEventDbApi.MusicEvent
            {
                Sender = Assembly.GetEntryAssembly()?.FullName,
                TimeStamp = DateTime.UtcNow,
                EventType = eventType.ToString()
            };
            return this.dbClient.AddEventAsync(dbEvent);
        }

        public Task PostEventAsync(EventType eventType, object payload)
        {
            var dbEvent = new MusicEventDbApi.MusicEvent
            {
                Sender = Assembly.GetEntryAssembly()?.FullName,
                TimeStamp = DateTime.UtcNow,
                EventType = eventType.ToString(),
                PayloadJson = JsonConvert.SerializeObject(payload),
                PayloadName = nameof(payload),
                PayloadType = payload.GetType().Name
            };
            return this.dbClient.AddEventAsync(dbEvent);
        }

        public Task PostEventAsync(EventType eventType, string payload)
        {
            var dbEvent = new MusicEventDbApi.MusicEvent
            {
                Sender = Assembly.GetEntryAssembly()?.FullName,
                TimeStamp = DateTime.UtcNow,
                EventType = eventType.ToString(),
                PayloadJson = payload,
                PayloadName = nameof(payload),
                PayloadType = payload.GetType().Name
            };
            return this.dbClient.AddEventAsync(dbEvent);
        }

        public async Task<bool> PutEventsAsync(IEnumerable<SharedDomain.InfraEvents.MusicEvent> events)
        {
            try
            {
                foreach (var @event in events)
                {
                    var dbEvent = new MusicEventDbApi.MusicEvent
                    {
                        Sender = @event.Sender,
                        TimeStamp = @event.TimeStamp,
                        EventType = @event.EventType,
                        PayloadJson = @event.PayloadJson,
                        PayloadName = @event.PayloadName,
                        PayloadType = @event.PayloadType
                    };
                    await this.dbClient.AddEventAsync(dbEvent);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
