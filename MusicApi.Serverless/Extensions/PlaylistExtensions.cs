using MusicDbApi.Models;
using SharedDomain.Domain;
using System.Collections.Generic;
using System.Linq;

namespace MusicApi.Serverless.Extensions
{
    public static class PlaylistExtensions
    {
        public static FullPlaylistViewModel ToFullViewModel(this Playlist playlist)
        {
            return new FullPlaylistViewModel
            {
                Featured = playlist.Featured,
                Id = playlist.Id,
                Name = playlist.Name,
                PictureUrl = playlist.PictureUrl,
                Songs = playlist.Songs.ConvertTo<List<SongViewModel>>()
            };
        }

        public static PlaylistViewModel ToSimpleViewModel(this Playlist playlist)
        {
            var viewModel = playlist.ConvertTo<PlaylistViewModel>();
            viewModel.KeyWords =
                string.Join(" ",
                string.Join(" ", playlist.Songs.Select(s => s.Artist)),
                string.Join(" ", playlist.Songs.Select(s => s.Title)));
            return viewModel;
        }
    }
}