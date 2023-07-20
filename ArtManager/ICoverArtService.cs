using MusicDbApi.Models;
using System.Threading;
using System.Threading.Tasks;

namespace ArtManager
{
    public interface ICoverArtService
    {
        Task<string> GetImageForPlaylistAsync(Playlist playlist, CancellationToken token = default);
    }
}