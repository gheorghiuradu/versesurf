using GamePlaying.Domain.PlaylistMetadataAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MusicDbApi;
using MusicDbApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GamePlaying.Repositories
{
    public class MetadataClient
    {
        private readonly IServiceProvider serviceProvider;

        public MetadataClient(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public async Task<bool> UpdatePlaylistMetadata(IEnumerable<PlaylistMetadata> playlistsMetadata)
        {
            try
            {
                using var scope = this.serviceProvider.CreateScope();
                var musicDbClient = scope.ServiceProvider.GetRequiredService<MusicDbClient>();

                var playlists = playlistsMetadata.Select(pm => new Playlist
                {
                    Id = pm.Id,
                    Votes = pm.Votes,
                    Plays = pm.Plays
                });

                await musicDbClient.IncrementPlaylistMetadataAsync(playlists);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}