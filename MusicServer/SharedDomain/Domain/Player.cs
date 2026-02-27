using System;

namespace SharedDomain
{
    public class Player : IEquatable<Player>
    {
        public string Id { get; set; }

        public string Nick { get; set; }

        public string Code { get; set; }

        public string CharacterCode { get; set; }
        public string ColorCode { get; set; }
        public bool IsConnected { get; set; }

        public bool Equals(Player other)
        {
            return string.Equals(this.Id, other.Id);
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }
    }
}