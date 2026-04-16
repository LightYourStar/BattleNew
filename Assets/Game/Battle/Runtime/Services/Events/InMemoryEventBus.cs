using System;
using System.Collections.Generic;

namespace Game.Battle.Runtime.Services.Events
{
    /// <summary>
    /// Simple event hub for local runtime messaging.
    /// </summary>
    public sealed class InMemoryEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        public void Publish<TEvent>(TEvent evt)
        {
            Type eventType = typeof(TEvent);
            if (!_handlers.TryGetValue(eventType, out List<Delegate> handlers))
            {
                return;
            }

            for (int i = 0; i < handlers.Count; i++)
            {
                if (handlers[i] is Action<TEvent> callback)
                {
                    callback.Invoke(evt);
                }
            }
        }

        public IDisposable Subscribe<TEvent>(Action<TEvent> handler)
        {
            Type eventType = typeof(TEvent);
            if (!_handlers.TryGetValue(eventType, out List<Delegate> handlers))
            {
                handlers = new List<Delegate>();
                _handlers.Add(eventType, handlers);
            }

            handlers.Add(handler);
            return new Subscription(() => handlers.Remove(handler));
        }

        private sealed class Subscription : IDisposable
        {
            private readonly Action _disposeAction;
            private bool _disposed;

            public Subscription(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                _disposeAction.Invoke();
            }
        }
    }
}
