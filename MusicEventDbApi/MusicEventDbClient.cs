using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MusicEventDbApi
{
    public class MusicEventDbClient
    {
        private const string EventCollectionName = "events";
        private readonly FirestoreDb db;

        public MusicEventDbClient(string projectId)
        {
            this.db = FirestoreDb.Create(projectId);
        }

        public Task AddEventAsync(MusicEvent @event)
        {
            if (@event.TimeStamp.Kind != DateTimeKind.Utc)
            {
                @event.TimeStamp = @event.TimeStamp.ToUniversalTime();
            }
            return this.db.Collection(EventCollectionName).AddAsync(@event);
        }

        public async ValueTask<List<MusicEvent>> GetEventsByDateAsync(
            DateTime start,
            DateTime end,
            IEnumerable<string> eventTypes = null,
            CancellationToken cancellationToken = default)
        {
            var query = this.db.Collection(EventCollectionName)
                .WhereGreaterThanOrEqualTo(nameof(MusicEvent.TimeStamp), start)
                .WhereLessThanOrEqualTo(nameof(MusicEvent.TimeStamp), end);

            if (!(eventTypes is null))
            {
                query = query.WhereIn(nameof(MusicEvent.EventType), eventTypes);
            }

            var snapshots = await query.GetSnapshotAsync(cancellationToken);

            return snapshots.Documents.Select(d => d.ConvertTo<MusicEvent>()).ToList();
        }
    }
}