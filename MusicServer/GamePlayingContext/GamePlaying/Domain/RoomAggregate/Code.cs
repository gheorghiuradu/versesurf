using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Domain.RoomAggregate
{
    public class Code : ValueObject
    {
        public virtual string Value { get; }

        internal Code(string value)
        {
            this.Value = value;
        }

        public static Result<Code, Error> Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return Result.Failure<Code, Error>(Errors.Room.NullOrEmptyCode());
            }

            if (value.Length != 5)
            {
                return Result.Failure<Code, Error>(Errors.Room.InvalidCode("The code should be 5 characters long."));
            }

            var upperValue = value.Trim().ToUpper();

            // TODO: other code business rules

            return Result.Ok<Code, Error>(new Code(upperValue));
        }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return this.Value;
        }
    }
}
