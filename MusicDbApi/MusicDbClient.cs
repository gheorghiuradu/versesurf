using Microsoft.EntityFrameworkCore;
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
        private readonly MusicDbContext db;
        private readonly Random random = new Random(DateTime.Now.Millisecond);

        public MusicDbClient(MusicDbContext db)
        {
            this.db = db;
        }

        public async Task<string> AddPlaylistAsync(Playlist playlist, CancellationToken token = default)
        {
            this.EnsureSongsHaveIds(playlist.Songs);

            if (string.IsNullOrEmpty(playlist.Id))
            {
                playlist.Id = Guid.NewGuid().ToString();
            }
            playlist.AddedAt = DateTime.UtcNow;
            playlist.UpdatedAt = DateTime.UtcNow;

            this.db.Playlists.Add(playlist);
            await this.db.SaveChangesAsync(token);

            return playlist.Id;
        }

        public async Task UpdatePlaylistAsync(Playlist playlist, CancellationToken token = default)
        {
            this.EnsureSongsHaveIds(playlist.Songs);
            playlist.UpdatedAt = DateTime.UtcNow;

            this.db.Playlists.Update(playlist);
            await this.db.SaveChangesAsync(token);
        }

        public async Task IncrementPlaylistMetadataAsync(IEnumerable<Playlist> playlists, CancellationToken token = default)
        {
            foreach (var playlist in playlists)
            {
                var existing = await this.db.Playlists.FindAsync(new object[] { playlist.Id }, token);
                if (existing != null)
                {
                    existing.Votes += playlist.Votes;
                    existing.Plays += playlist.Plays;
                }
            }
            await this.db.SaveChangesAsync(token);
        }

        public async Task<List<Playlist>> GetEnabledPlaylistsAsync(string language = null, bool includeExplicit = false, CancellationToken token = default)
        {
            var query = this.db.Playlists
                .Include(p => p.Songs)
                .Where(p => p.Enabled);

            var playlists = await query.ToListAsync(token);

            IEnumerable<Playlist> result = playlists;
            if (!includeExplicit)
            {
                result = result.Where(p => !p.Songs.Any(s => s.IsExplicit));
            }
            if (!string.IsNullOrEmpty(language))
            {
                result = result.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));
            }

            return result.OrderByDescending(p => p.Featured).ThenByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Plays).ToList();
        }

        public async Task<List<Playlist>> GetAllPlaylistsAsync(string language = null, bool includeExplicit = false, CancellationToken token = default)
        {
            var playlists = await this.db.Playlists
                .Include(p => p.Songs)
                .ToListAsync(token);

            IEnumerable<Playlist> result = playlists;
            if (!includeExplicit)
            {
                result = result.Where(p => !p.Songs.Any(s => s.IsExplicit));
            }
            if (!string.IsNullOrEmpty(language))
            {
                result = result.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));
            }

            return result.OrderByDescending(p => p.Featured).ThenByDescending(p => p.UpdatedAt).ThenByDescending(p => p.Plays).ToList();
        }

        public async Task<Playlist> GetRandomPlaylistAsync(string language = null, bool includeExplicit = false, bool onlyEnabled = true)
        {
            var query = this.db.Playlists.Include(p => p.Songs).AsQueryable();
            if (onlyEnabled)
            {
                query = query.Where(p => p.Enabled);
            }
            var playlists = await query.ToListAsync();

            IEnumerable<Playlist> result = playlists;
            if (!includeExplicit)
            {
                result = result.Where(p => !p.Songs.Any(s => s.IsExplicit));
            }
            if (!string.IsNullOrEmpty(language))
            {
                result = result.Where(p => string.Equals(p.Language, language, StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(p.Language));
            }

            var list = result.ToList();
            return list.ElementAt(random.Next(0, list.Count));
        }

        public async Task<Playlist> GetPlaylistByIdAsync(string id, CancellationToken token = default)
        {
            return await this.db.Playlists
                .Include(p => p.Songs)
                .FirstOrDefaultAsync(p => p.Id == id, token);
        }

        public async Task<List<Playlist>> GetPlaylistsByIdAsync(IEnumerable<string> ids, CancellationToken token = default)
        {
            return await this.db.Playlists
                .Include(p => p.Songs)
                .Where(p => ids.Contains(p.Id))
                .ToListAsync(token);
        }

        public async Task<Playlist> GetPlaylistBySpotifyIdAsync(string id, CancellationToken token = default)
        {
            return await this.db.Playlists
                .Include(p => p.Songs)
                .FirstOrDefaultAsync(p => p.SpotifyId == id, token);
        }

        public async Task<bool> PlaylistExistsByIdAsync(string id, CancellationToken token = default)
        {
            return await this.db.Playlists.AnyAsync(p => p.Id == id, token);
        }

        public async Task<bool> PlaylistExistsBySpotifyIdAsync(string id, CancellationToken token = default)
        {
            return await this.db.Playlists.AnyAsync(p => p.SpotifyId == id, token);
        }

        public async Task DeletePlaylistAsync(string id, CancellationToken token = default)
        {
            var playlist = await this.db.Playlists.FindAsync(new object[] { id }, token);
            if (playlist != null)
            {
                this.db.Playlists.Remove(playlist);
                await this.db.SaveChangesAsync(token);
            }
        }

        private void EnsureSongsHaveIds(IEnumerable<Song> songs)
        {
            if (songs == null) return;
            foreach (var song in songs)
            {
                if (string.IsNullOrEmpty(song.Id))
                {
                    song.Id = Guid.NewGuid().ToString();
                }
            }
        }
    }
}