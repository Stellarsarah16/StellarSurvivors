using System;
using System.Collections.Generic;

namespace StellarSurvivors.Core
{
    /// <summary>
    /// A generic, type-safe event bus for decoupling game systems.
    /// </summary>
    public class EventManager
    {
        // A dictionary mapping an Event Type (like typeof(PlayerDiedEvent))
        // to a list of delegates (functions) to call.
        private readonly Dictionary<Type, List<Delegate>> _listeners = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribe to an event.
        /// </summary>
        /// <param name="listener">The action to execute when the event is published.</param>
        /// <typeparam name="T">The type of event to listen for (must implement IEvent).</typeparam>
        public void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);
            if (!_listeners.ContainsKey(eventType))
            {
                _listeners[eventType] = new List<Delegate>();
            }
            _listeners[eventType].Add(listener);
        }

        /// <summary>
        /// Unsubscribe from an event.
        /// </summary>
        /// <param name="listener">The specific action to remove.</param>
        /// <typeparam name="T">The type of event.</typeparam>
        public void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            Type eventType = typeof(T);
            if (_listeners.ContainsKey(eventType))
            {
                _listeners[eventType].Remove(listener);
            }
        }

        /// <summary>
        /// Publish an event to all subscribers.
        /// </summary>
        /// <param name="eventToPublish">The event object with its data.</param>
        /// <typeparam name="T">The type of event.</typeparam>
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