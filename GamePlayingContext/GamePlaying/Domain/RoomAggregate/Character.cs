using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Domain.RoomAggregate
{
    public class Character : ValueObject
    {
        public string Value { get; }

        internal Character(string value)
        {
            this.Value = value;
        }

        public static Result<Character, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<Character, Error>(Errors.GameSetup.InvalidEmoji());
            }

            // TODO: other code business rules

            return Result.Ok<Character, Error>(new Character(value));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
