using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace PraganoidSystems.Events
{
    public abstract class BaseGameEvent<T> : ScriptableObject
    {
        private Action<T> OnEventTriggered;

        public void Trigger(T value) => OnEventTriggered?.Invoke(value);

        public void Subscribe(Action<T> handler)
        {
            OnEventTriggered += handler;
        }

        public void Unsubscribe(Action<T> handler)
        {
            OnEventTriggered -= handler;

            var reference = handler.Target != null
                ? handler.Target.ToString()
                : handler.Method.Name;
        }
    }
}