using FirestoreExtensions;
using Google.Cloud.Firestore;
using MusicDbApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MusicDbApi
{
    public class MusicDbClient
    {
        private readonly FirestoreDb db;
        private readonly Random random = new Random(DateTime.Now.Millisecond);

        private readonly Dictionary<Type, string> namedCollections = new Dictionary<Type, string>
        {
            {typeof(Playlist), "playlists" }
        };

        public MusicDbClient(string projectId)
        {
            this.db = FirestoreDb.Create(projectId);
        }

        public async Task<string> AddPlaylistAsync(Playlist playlist, CancellationToken token = default)
        {
            this.EnsureSongsHaveIds(playlist.Songs);

            var document = await this.Collection<Playlist>().AddAsync(playlist, token);

            return document.Id;
        }

        public Task UpdatePlaylistAsync(Playlist playlist, CancellationToken token = default)
        {
            this.EnsureSongsHaveIds(playlist.Songs);

            var document = this.Collection<Playlist>().Document(playlist.Id);

            return document.SetAsync(playlist, SetOptions.MergeAll, token);
        }

        public Task<IList<WriteResult>> IncrementPlaylistMetadataAsync(IEnumerable<Playlist> playlists, CancellationToken token = default)
        {
            var batch = db.StartBatch();

            foreach (var playlist in playlists)
            {
                var documentRef = this.Collection<Playlist>().Document(playlist.Id);

                var metadataUpdates = new Dictionary<string, object>
                {
                    { nameof(Playlist.Votes), FieldValue.Increment(playlist.Votes) },
                    { nameof(Playlist.Plays), FieldValue.Increment(playlist.Plays) }
                };

                batch.Update(documentRef, metadataUpdates);
            }

            return batch.CommitAsync(token);
        }

        public async Task<List<Playlist>> GetEnabledPlaylistsAsync(string language = null, bool includeExplicit = false, CancellationToken token = default)
        {
            var documents = await this.Collection<Playlist>()
                .WhereEqualTo(PropertyPathProvider<Playlist>.GetPropertyPath(playlist => playlist.Enabled), true)
                .GetSnapshotAsync(token);

            var playlists = documents.ConvertTo<Playlist>();
            if (!includeExplicit)
            {
                playlists = playlists.Where(p => !p.Songs.Any(s => s.IsExplicit));
            }
            if (!string.IsNullOrEmpty(language))
            {
                playlists = playlists.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));
            }

            return playlists.OrderByDescending(p => p.Featured).ThenByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Plays).ToList();
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync(string language = null, bool includeExplicit = false, CancellationToken token = default)
        {
            var documents = await this.Collection<Playlist>()
                .GetSnapshotAsync(token);

            var playlists = documents.ConvertTo<Playlist>();
            if (!includeExplicit)
            {
                playlists = playlists.Where(p => !p.Songs.Any(s => s.IsExplicit));
            }
            if (!string.IsNullOrEmpty(language))
            {
                playlists = playlists.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));
            }

            return playlists.OrderByDescending(p => p.Featured).ThenByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Plays).ToList();
        }

        public async Task<Playlist> GetRandomPlaylistAsync(string language = null, bool includeExplicit = false, bool onlyEnabled = true)
        {
            Query query = this.Collection<Playlist>();
            if (onlyEnabled)
            {
                query = query.WhereEqualTo(PropertyPathProvider<Playlist>.GetPropertyPath(p => p.Enabled), true);
            }
            var documents = await query.GetSnapshotAsync();

            var playlists = documents.ConvertTo<Playlist>();
            if (!includeExplicit)
            {
                playlists = playlists.Where(p => !p.Songs.Any(s => s.IsExplicit));
            }
            if (!string.IsNullOrEmpty(language))
            {
                playlists = playlists.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));
            }

            return playlists.ElementAt(random.Next(0, playlists.Count() - 1));
        }

        public async Task<Playlist> GetPlaylistByIdAsync(string id, CancellationToken token = default)
        {
            var document = await this.Collection<Playlist>().Document(id).GetSnapshotAsync(token);

            return document.ConvertTo<Playlist>();
        }

        public async Task<List<Playlist>> GetPlaylistsByIdAsync(IEnumerable<string> ids, CancellationToken token = default)
        {
            var documents = await this.Collection<Playlist>()
                            .WhereIn(FieldPath.DocumentId, ids)
                            .GetSnapshotAsync(token);

            return documents.ConvertTo<Playlist>().ToList();
        }

        public async Task<Playlist> GetPlaylistBySpotifyIdAsync(string id, CancellationToken token = default)
        {
            var documents = await this.Collection<Playlist>()
                .WhereEqualTo(PropertyPathProvider<Playlist>.GetPropertyPath(p => p.SpotifyId), id)
                .GetSnapshotAsync(token);

            return documents.ConvertTo<Playlist>().FirstOrDefault(p => string.Equals(id, p.SpotifyId));
        }

        public async Task<bool> PlaylistExistsByIdAsync(string id, CancellationToken token = default)
        {
            var document = await this.Collection<Playlist>().Document(id).GetSnapshotAsync(token);

            return document.Exists;
        }

        public async Task<bool> PlaylistExistsBySpotifyIdAsync(string id, CancellationToken token = default)
        {
            var documents = await this.Collection<Playlist>()
                .WhereEqualTo(PropertyPathProvider<Playlist>.GetPropertyPath(p => p.SpotifyId), id)
                .GetSnapshotAsync(token);

            return documents.ConvertTo<Playlist>().Any(p => string.Equals(p.SpotifyId, id));
        }

        public Task DeletePlaylistAsync(string id, CancellationToken token = default)
        {
            var doc = this.Collection<Playlist>().Document(id);
            return doc.DeleteAsync(cancellationToken: token);
        }

        private void EnsureSongsHaveIds(IEnumerable<Song> songs)
        {
            foreach (var song in songs)
            {
                if (string.IsNullOrEmpty(song.Id))
                {
                    song.Id = Guid.NewGuid().ToString();
                }
            }
        }

        private CollectionReference Collection<T>()
        {
            return this.db.Collection(this.namedCollections[typeof(T)]);
        }
    }
}