using CSharpFunctionalExtensions;
using GamePlaying.Infrastructure;

namespace GamePlaying.Domain.PlaylistMetadataAggregate
{
    public class PlaylistMetadata : Entity
    {
        private PlaylistMetadata(string id)
        {
            this.Id = id;
        }

        public int Votes { get; private set; }

        public int Plays { get; private set; }

        public static Result<PlaylistMetadata, Error> Create(string playlistId)
        {
            var playlist = new PlaylistMetadata(playlistId);
            return Result.Ok<PlaylistMetadata, Error>(playlist);
        }

        public void IncreaseVotes()
        {
            this.Votes++;
        }

        public void IncreasePlays()
        {
            this.Plays++;
        }

        public void Clear()
        {
            this.Votes = 0;
            this.Plays = 0;
        }
    }
}