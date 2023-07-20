using SharedDomain;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Extensions
{
    public class AnswerComparer : IEqualityComparer<Answer>
    {
        public bool Equals(Answer x, Answer y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Answer obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}