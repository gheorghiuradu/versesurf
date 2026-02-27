using CSharpFunctionalExtensions;
using GamePlaying.Infrastructure;

namespace GamePlaying.Domain.GameAggregate
{
    public class Playlist : Entity
    {
        public string Name { get; }
        public string PictureUrl { get; }
        public bool Featured { get; set; }

        private Playlist(string id, string name, string pictureUrl, bool featured)
        {
            this.Id = id;
            this.Name = name;
            this.PictureUrl = pictureUrl;
            this.Featured = featured;
        }

        public static Result<Playlist, Error> Create(string id, string name, string pictureUrl, bool featured)
        {
            // TODO: validate input

            return new Playlist(id, name, pictureUrl, featured);
        }
    }
}