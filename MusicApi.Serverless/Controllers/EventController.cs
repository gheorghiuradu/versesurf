using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicApi.Serverless.Extensions;
using MusicEventDbApi;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DomainMusicEvent = SharedDomain.InfraEvents.MusicEvent;

namespace MusicApi.Serverless.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly MusicEventDbClient eventDbClient;

        public EventController(MusicEventDbClient eventDbClient)
        {
            this.eventDbClient = eventDbClient;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Post([FromBody] DomainMusicEvent musicEvent)
        {
            await this.InsertEvent(musicEvent);
            return this.Ok();
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Put([FromBody] IEnumerable<DomainMusicEvent> musicEvents)
        {
            foreach (var item in musicEvents)
            {
                await this.InsertEvent(item);
            }

            return this.Ok();
        }

        private Task InsertEvent(DomainMusicEvent musicEvent)
        {
            if (musicEvent.TimeStamp.Equals(DateTime.MinValue))
            {
                musicEvent.TimeStamp = DateTime.UtcNow;
            }
            return this.eventDbClient.AddEventAsync(musicEvent.ConvertTo<MusicEvent>());
        }
    }
}