using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Domain.RoomAggregate
{
    public class Color : ValueObject
    {
        public string Value { get; }

        internal Color(string value)
        {
            this.Value = value;
        }

        public static Result<Color, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<Color, Error>(Errors.GameSetup.InvalidColor());
            }

            // TODO: other code business rules

            return Result.Ok<Color, Error>(new Color(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
