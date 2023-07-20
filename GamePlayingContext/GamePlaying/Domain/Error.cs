using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Domain
{
    public sealed class Error : ValueObject
    {
        internal Error(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public string Code { get; }

        public string Message { get; }

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Code;
        }
    }
}
