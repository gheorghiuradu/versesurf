using SharedDomain.InfraEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Assets.Scripts.Services
{
    public class CustomEventService
    {
        private readonly PlayFabService playFabService;
        private readonly MusicClient musicClient;
        private List<MusicEvent> unpushedEvents = new List<MusicEvent>();

        public CustomEventService(PlayFabService playFabService, MusicClient musicClient)
        {
            this.playFabService = playFabService;
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
            var savedEvents = await this.playFabService.GetCustomEventsAsync();
            if (savedEvents.Any())
            {
                this.unpushedEvents.AddRange(savedEvents);
            }

            if (unpushedEvents.Count > 0)
            {
                try
                {
                    var result = await this.musicClient.PushEventsAsync(unpushedEvents);
                    if (!result.IsSuccess)
                    {
                        await this.playFabService.SaveCustomEventsAsync(unpushedEvents);
                    }
                    else
                    {
                        await this.playFabService.ClearCustomEventsAsync();
                        this.unpushedEvents.Clear();
                    }
                }
                catch (System.Exception)
                {
                    await this.playFabService.SaveCustomEventsAsync(unpushedEvents);
                }
            }
        }
    }
}