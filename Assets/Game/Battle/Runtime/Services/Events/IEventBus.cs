using System;

namespace Game.Battle.Runtime.Services.Events
{
    public interface IEventBus
    {
        void Publish<TEvent>(TEvent evt);

        IDisposable Subscribe<TEvent>(Action<TEvent> handler);
    }
}
