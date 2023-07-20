using GamePlaying.Domain.PlaylistMetadataAggregate;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace GamePlaying.Repositories
{
    public class PlaylistMetadataBuffer : IPlaylistMetadataBuffer
    {
        private readonly ConcurrentDictionary<string, PlaylistMetadata> playlists = new ConcurrentDictionary<string, PlaylistMetadata>();
        private readonly MetadataClient metadataClient;

        public PlaylistMetadataBuffer(MetadataClient metadataClient)
        {
            this.metadataClient = metadataClient;
        }

        public void AddPlaylistMetadata(PlaylistMetadata playlist)
        {
            this.playlists.TryAdd(playlist.Id, playlist);
        }

        public PlaylistMetadata GetPlaylistMetadata(string playlistId)
        {
            this.playlists.TryGetValue(playlistId, out var playlist);
            return playlist;
        }

        public void RemovePlaylistMetadata(string playlistId)
        {
            this.playlists.TryRemove(playlistId, out var _);
        }

        public void Push()
        {
            Task.Run(async () =>
                    {
                        var success = await this.metadataClient.UpdatePlaylistMetadata(playlists.Values);
                        if (!success)
                        {
                            return;
                        }

                        foreach (var playlist in playlists.Values)
                        {
                            playlist.Clear();
                        }
                    })
                .Forget();
        }
    }
}