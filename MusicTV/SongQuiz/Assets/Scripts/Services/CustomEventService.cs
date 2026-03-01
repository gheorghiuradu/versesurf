using SharedDomain.InfraEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.Services
{
    public class CustomEventService
    {
        private readonly MusicClient musicClient;
        private List<MusicEvent> unpushedEvents = new List<MusicEvent>();

        public CustomEventService(MusicClient musicClient)
        {
            this.musicClient = musicClient;
        }

        public Task AddEventAsync(MusicEvent @event, bool tryPushImmediately = false)
        {
            this.unpushedEvents.Add(@event);
            return tryPushImmediately ? this.TryPushRemainingEventsAsync() : Task.CompletedTask;
        }

        public Task AddEventsAsync(IEnumerable<MusicEvent> events, bool tryPushImmediately = false)
        {
            this.unpushedEvents.AddRange(events);
            return tryPushImmediately ? this.TryPushRemainingEventsAsync() : Task.CompletedTask;
        }

        public async Task TryPushRemainingEventsAsync()
        {
        }
    }
}