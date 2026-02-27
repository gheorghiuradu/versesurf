using GamePlaying.Domain.PlaylistMetadataAggregate;
using System;

namespace GamePlaying.Application
{
    public class MetadataService
    {
        private readonly IPlaylistMetadataBuffer playlistMetadataBuffer;

        public MetadataService(IPlaylistMetadataBuffer playlistMetadataBuffer)
        {
            this.playlistMetadataBuffer = playlistMetadataBuffer;
        }

        public void CountVoteForPlaylist(string playlistId)
        {
            try
            {
                var playlist = this.GetOrCreate(playlistId);
                playlist.IncreaseVotes();
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }

        public void CountPlayForPlaylist(string playlistId)
        {
            try
            {
                var playlist = this.GetOrCreate(playlistId);
                playlist.IncreasePlays();
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }

        public void PushPlaylistsMetadata()
        {
            try
            {
                this.playlistMetadataBuffer.Push();
            }
            catch (Exception ex)
            {
                // TODO: log
            }
        }

        private PlaylistMetadata GetOrCreate(string playlistId)
        {
            var playlist = this.playlistMetadataBuffer.GetPlaylistMetadata(playlistId);
            if (playlist is null)
            {
                playlist = PlaylistMetadata.Create(playlistId).Value;
                this.playlistMetadataBuffer.AddPlaylistMetadata(playlist);
            }

            return playlist;
        }
    }
}