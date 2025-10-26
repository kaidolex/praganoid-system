using System;
using UnityEngine;
using UnityEngine.Events;

namespace PraganoidSystems.Events
{
    public abstract class BaseGameEventListner<T> : MonoBehaviour
    {
        [SerializeField] private BaseGameEvent<T> eventChannel;
        [SerializeField] private UnityEvent<T> response;

        private void OnEnable() => eventChannel.Subscribe(OnEventTriggered);

        private void OnDisable() => eventChannel.Unsubscribe(OnEventTriggered);

        private void OnEventTriggered(T param) => response?.Invoke(param);
    }
}
