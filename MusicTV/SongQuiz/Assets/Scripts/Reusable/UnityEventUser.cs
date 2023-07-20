using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Reusable
{
    public abstract class UnityEventUser : MonoBehaviour
    {
        public List<KeyValuePair<int, Delegate>> Actions { get; } = new List<KeyValuePair<int, Delegate>>();

        public UnityAction<T> AddAction<T>(int hashCode, UnityAction<T> action)
        {
            this.Actions.Add(new KeyValuePair<int, Delegate>(hashCode, action));
            return action;
        }

        public UnityAction AddAction(int hashCode, UnityAction action)
        {
            this.Actions.Add(new KeyValuePair<int, Delegate>(hashCode, action));
            return action;
        }
    }
}