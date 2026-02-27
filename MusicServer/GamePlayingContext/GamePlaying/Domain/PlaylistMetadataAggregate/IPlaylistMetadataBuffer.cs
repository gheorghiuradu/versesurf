using System.Threading.Tasks;

namespace GamePlaying.Domain.PlaylistMetadataAggregate
{
    public interface IPlaylistMetadataBuffer
    {
        void AddPlaylistMetadata(PlaylistMetadata playlistMetadata);

        PlaylistMetadata GetPlaylistMetadata(string playlistMetadataId);

        void RemovePlaylistMetadata(string playlistMetadataId);

        void Push();
    }
}
