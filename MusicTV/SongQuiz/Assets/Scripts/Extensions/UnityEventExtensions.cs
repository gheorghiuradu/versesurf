using Assets.Scripts.Reusable;
using System.Linq;
using UnityEngine.Events;

namespace Assets.Scripts.Extensions
{
    public static class UnityEventExtensions
    {
        public static void RemoveAllListenersFrom(this UnityEvent @event, UnityEventUser eventUser)
        {
            var actions = eventUser.Actions.Where(kv => string.Equals(kv.Key, @event.GetHashCode())).Select(kv => kv.Value);
            if (actions.Any())
            {
                foreach (var action in actions)
                {
                    @event.RemoveListener(action as UnityAction);
                }
            }
        }

        public static void RemoveAllListenersFrom<T0>(this UnityEvent<T0> @event, UnityEventUser eventUser)
        {
            var actions = eventUser.Actions.Where(kv => string.Equals(kv.Key, @event.GetHashCode())).Select(kv => kv.Value);
            if (actions.Any())
            {
                foreach (var action in actions)
                {
                    @event.RemoveListener(action as UnityAction<T0>);
                }
            }
        }

        public static void AddListener<T0>(this UnityEvent<T0> @event, UnityEventUser eventUser, UnityAction<T0> call)
        {
            eventUser.AddAction(@event.GetHashCode(), call);
            @event.AddListener(call);
        }

        public static void AddListener(this UnityEvent @event, UnityEventUser eventUser, UnityAction call)
        {
            eventUser.AddAction(@event.GetHashCode(), call);
            @event.AddListener(call);
        }
    }
}