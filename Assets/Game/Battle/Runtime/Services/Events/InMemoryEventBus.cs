using System;
using System.Collections.Generic;

namespace Game.Battle.Runtime.Services.Events
{
    /// <summary>
    /// 进程内事件总线：实现简单、依赖少，适合早期骨架与单机验证。
    /// <para>
    /// 注意：
    /// - 这不是线程安全实现；Unity 主线程使用没问题。
    /// - 订阅者在回调中再次 Publish/Subscribe 可能引发迭代问题；后续可引入队列化派发。
    /// </para>
    /// </summary>
    public sealed class InMemoryEventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new();

        /// <inheritdoc />
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

        /// <inheritdoc />
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

        /// <summary>订阅句柄：Dispose 即取消订阅。</summary>
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
