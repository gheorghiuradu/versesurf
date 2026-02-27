using System;
using System.Collections.Generic;
using System.Text;

namespace GamePlaying.Infrastructure
{
    public abstract class Entity : IEquatable<Entity>
    {
        private string id;

        public virtual string Id 
        { 
            get 
            {
                if (string.IsNullOrWhiteSpace(this.id)) 
                {
                    this.id = Guid.NewGuid().ToString(); 
                } 
                
                return this.id;
            }

            protected set { this.id = value; } 
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Entity);
        }

        public bool Equals(Entity other)
        {
            return other != null &&
                Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }

        public static bool operator ==(Entity entity1, Entity entity2)
        {
            return EqualityComparer<Entity>.Default.Equals(entity1, entity2);
        }

        public static bool operator !=(Entity entity1, Entity entity2)
        {
            return !(entity1 == entity2);
        }
    }
}
