using CSharpFunctionalExtensions;
using System.Collections.Generic;
using System.Linq;

namespace GamePlaying.Domain.RoomAggregate
{
    public class GameSetup : ValueObject
    {
        public IEnumerable<Color> Colors { get; }

        public IEnumerable<Character> Characters { get; }

        private GameSetup(
            IEnumerable<Color> colors,
            IEnumerable<Character> characters)
        {
            this.Colors = colors;
            this.Characters = characters;
        }

        public static Result<GameSetup, Error> Create(
            List<string> colorValues,
            List<string> characterValues)
        {
            // TODO: validate emojis and colors BL

            var colors = colorValues.Select(c => Color.Create(c).Value);
            var characters = characterValues.Select(e => Character.Create(e).Value);

            var gameSetup = new GameSetup(colors, characters);

            // TODO: other GameSetup business rules

            return Result.Ok<GameSetup, Error>(gameSetup);
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            foreach (var color in this.Colors)
            {
                yield return color.Value;
            }

            foreach (var emoji in this.Characters)
            {
                yield return emoji.Value;
            }
        }
    }
}