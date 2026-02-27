using System;
using System.Collections.Generic;

namespace SharedDomain
{
    public class AnswerNameComparer : IEqualityComparer<Answer>
    {
        public bool Equals(Answer x, Answer y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.InvariantCultureIgnoreCase);
        }

        public int GetHashCode(Answer obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}