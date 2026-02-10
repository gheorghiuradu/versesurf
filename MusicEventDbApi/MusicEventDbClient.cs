using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MusicEventDbApi
{
    public class MusicEventDbClient
    {
        private readonly MusicEventDbContext db;

        public MusicEventDbClient(MusicEventDbContext db)
        {
            this.db = db;
        }

        public async Task AddEventAsync(MusicEvent @event)
        {
            if (@event.TimeStamp.Kind != DateTimeKind.Utc)
            {
                @event.TimeStamp = @event.TimeStamp.ToUniversalTime();
            }
            this.db.Events.Add(@event);
            await this.db.SaveChangesAsync();
        }

        public async ValueTask<List<MusicEvent>> GetEventsByDateAsync(
            DateTime start,
            DateTime end,
            IEnumerable<string> eventTypes = null,
            CancellationToken cancellationToken = default)
        {
            var query = this.db.Events
                .Where(e => e.TimeStamp >= start && e.TimeStamp <= end);

            if (eventTypes != null)
            {
                query = query.Where(e => eventTypes.Contains(e.EventType));
            }

            return await query.ToListAsync(cancellationToken);
        }
    }
}