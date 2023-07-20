using Microsoft.Extensions.Logging;
using RestSharp;
using SharedDomain.InfraEvents;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApi.Serverless.Client
{
    public class MusicEventClient : MusicApiServerlessClient
    {
        public MusicEventClient(MusicApiServerlessOptions options)
            : base(options)
        {
        }

        public Task PostEventAsync(EventType eventType)
        {
            return this.PostEventAsync(new MusicEvent(eventType));
        }

        public Task PostEventAsync(EventType eventType, object payload)
        {
            return this.PostEventAsync(new MusicEvent(eventType, payload));
        }

        public Task PostEventAsync(EventType eventType, string payload)
        {
            return this.PostEventAsync(new MusicEvent(eventType, payload));
        }

        private async Task PostEventAsync(MusicEvent musicEvent)
        {
            this.logger.LogDebug($"Starting request for event {musicEvent.EventType}");

            var request = new RestRequest("/event", Method.POST);
            request.AddJsonBody(musicEvent);

            var response = await this.restClient.ExecuteAsync(request);

            this.LogIfError(response);
        }

        public async Task<bool> PutEventsAsync(IEnumerable<MusicEvent> events)
        {
            this.logger.LogDebug($"Starting request for {events.Count()} events");

            var request = new RestRequest("/event", Method.PUT);
            request.AddJsonBody(events);

            var response = await this.restClient.ExecuteAsync(request);

            this.LogIfError(response);

            return response.IsSuccessful;
        }
    }
}