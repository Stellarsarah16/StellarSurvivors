using System;
using System.Collections.Generic;

namespace StellarSurvivors.Core
{
    public class EventManager
    {
        // A dictionary mapping an Event Type
        // to a list of delegates (functions) to call.
        private readonly Dictionary<Type, List<Delegate>> _listeners = new Dictionary<Type, List<Delegate>>();
        
        public void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);
            if (!_listeners.ContainsKey(eventType))
            {
                _listeners[eventType] = new List<Delegate>();
            }
            _listeners[eventType].Add(listener);
        }
        
        public void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);
            if (_listeners.ContainsKey(eventType))
            {
                _listeners[eventType].Remove(listener);
            }
        }
        
        public void Publish<T>(T eventToPublish) where T : IEvent
        {
            Type eventType = typeof(T);
            if (_listeners.ContainsKey(eventType))
            {
                // Iterate over a copy in case a listener unsubscribes during iteration
                foreach (var listener in _listeners[eventType].ToArray())
                {
                    // Cast the delegate to the specific Action type and invoke it
                    ((Action<T>)listener)(eventToPublish);
                }
            }
        }
    }
}