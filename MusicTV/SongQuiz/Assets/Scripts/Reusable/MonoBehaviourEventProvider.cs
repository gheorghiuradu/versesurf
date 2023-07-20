using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Reusable
{
    public abstract class MonoBehaviourEventProvider : MonoBehaviour
    {
        public void RemoveAllListenersFrom(UnityEventUser unityEventUser)
        {
            foreach (var property in this.GetType().GetProperties().Where(p => p.PropertyType.Name.StartsWith(nameof(UnityEvent))))
            {
                var unityEvent = property.GetValue(this);
                var method = unityEvent.GetType().GetMethod(nameof(UnityEvent.RemoveListener));
                if (!(method is null))
                {
                    var actions = unityEventUser.Actions
                        .Where(kv => string.Equals(kv.Key, unityEvent.GetHashCode()))
                        .Select(kv => kv.Value);
                    if (actions.Any())
                    {
                        foreach (var action in actions)
                        {
                            method.Invoke(unityEvent, new[] { action });
                        }
                    }
                }
            }
        }
    }
}