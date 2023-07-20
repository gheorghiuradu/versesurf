using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MusicApi.Serverless.Extensions;
using MusicApi.Serverless.Models;
using MusicDbApi;
using MusicDbApi.Models;
using SharedDomain.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MusicApi.Serverless.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PlaylistController : ControllerBase
    {
        private readonly MusicDbClient musicDbClient;

        public PlaylistController(MusicDbClient musicDbClient)
        {
            this.musicDbClient = musicDbClient;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PlaylistViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FullPlaylistViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll(
            [FromQuery] PlaylistViewModelType type = PlaylistViewModelType.Simple,
            [FromQuery] bool includeExplicit = false,
            [FromQuery] string language = null)
        {
            var playlists = await this.musicDbClient.GetAllPlaylistsAsync(language, includeExplicit);

            return this.Ok(playlists.Select<Playlist, IPlaylistViewModel>(p => type switch
            {
                PlaylistViewModelType.Simple => p.ToSimpleViewModel(),
                PlaylistViewModelType.Full => p.ToFullViewModel(),
                _ => throw new System.NotImplementedException()
            }));
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<PlaylistViewModel>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FullPlaylistViewModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetEnabled(
            [FromQuery] PlaylistViewModelType type = PlaylistViewModelType.Simple,
            [FromQuery] bool includeExplicit = false,
            [FromQuery] string language = null)
        {
            var playlists = await this.musicDbClient.GetEnabledPlaylistsAsync(language, includeExplicit);

            return this.Ok(playlists.Select<Playlist, IPlaylistViewModel>(p => type switch
            {
                PlaylistViewModelType.Simple => p.ToSimpleViewModel(),
                PlaylistViewModelType.Full => p.ToFullViewModel(),
                _ => throw new System.NotImplementedException()
            }));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(PlaylistViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FullPlaylistViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(
            string id,
            [FromQuery] PlaylistViewModelType type = PlaylistViewModelType.Simple)
        {
            var playlist = await this.musicDbClient.GetPlaylistByIdAsync(id);

            return playlist is null ? (ActionResult)this.NotFound() : this.Ok(
                type switch
                {
                    PlaylistViewModelType.Simple => playlist.ToSimpleViewModel(),
                    PlaylistViewModelType.Full => playlist.ToFullViewModel(),
                    _ => throw new System.NotImplementedException()
                });
        }

        [HttpGet]
        [ProducesResponseType(typeof(PlaylistViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(FullPlaylistViewModel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRandom([FromQuery] bool includeExplicit = false, [FromQuery] bool onlyEnabled = true,
            [FromQuery] string language = null,
            [FromQuery] PlaylistViewModelType type = PlaylistViewModelType.Simple)
        {
            var playlist = await this.musicDbClient.GetRandomPlaylistAsync(language, includeExplicit, onlyEnabled);

            return playlist is null ? (ActionResult)this.NotFound() : this.Ok(
                type switch
                {
                    PlaylistViewModelType.Simple => playlist.ToSimpleViewModel(),
                    PlaylistViewModelType.Full => playlist.ToFullViewModel(),
                    _ => throw new System.NotImplementedException()
                });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Metadata([FromBody] IEnumerable<PlaylistMetadata> playlistsMetadata)
        {
            var playlists = playlistsMetadata.Select(m => new Playlist
            {
                Id = m.Id,
                Votes = m.Votes,
                Plays = m.Plays
            });

            var updateResults = await musicDbClient.IncrementPlaylistMetadataAsync(playlists);
            return updateResults.Any()
                ? (ActionResult)this.NoContent()
                : this.BadRequest();
        }
    }
}