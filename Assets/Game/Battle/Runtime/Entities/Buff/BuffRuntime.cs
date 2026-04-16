using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Buff
{
    /// <summary>
    /// Buff 运行时基类：提供通用的持续时间递减与过期判定。
    /// 具体 Buff 继承此类并实现 OnAdd/OnTick/OnRemove。
    /// </summary>
    public abstract class BuffRuntime : IBuff
    {
        public string BuffId { get; }
        public string OwnerId { get; }
        public float Duration { get; }
        public float Elapsed { get; private set; }
        public bool IsExpired => Elapsed >= Duration;

        protected BuffRuntime(string buffId, string ownerId, float duration)
        {
            BuffId = buffId;
            OwnerId = ownerId;
            Duration = duration;
        }

        public virtual void OnAdd(BattleContext context)
        {
        }

        public virtual void OnTick(BattleContext context, float deltaTime)
        {
            Elapsed += deltaTime;
        }

        public virtual void OnRemove(BattleContext context)
        {
        }
    }
}
