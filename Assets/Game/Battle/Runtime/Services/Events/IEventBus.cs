using System;

namespace Game.Battle.Runtime.Services.Events
{
    /// <summary>
    /// 事件总线：用于战斗内模块的松耦合通知（区别于命令系统的“确定性状态输入”）。
    /// </summary>
    public interface IEventBus
    {
        /// <summary>发布一个事件给所有订阅者。</summary>
        void Publish<TEvent>(TEvent evt);

        /// <summary>订阅事件；返回 IDisposable 用于取消订阅。</summary>
        IDisposable Subscribe<TEvent>(Action<TEvent> handler);
    }
}
