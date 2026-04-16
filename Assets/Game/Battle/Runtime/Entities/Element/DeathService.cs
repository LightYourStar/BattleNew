using Game.Battle.Runtime.Core;

namespace Game.Battle.Runtime.Entities.Element
{
    /// <summary>
    /// 死亡服务：统一死亡出口，负责死亡广播与回收。
    /// </summary>
    public sealed class DeathService
    {
        /// <summary>
        /// 当一个实体死亡时调用（通过事件总线广播，后续可挂接掉落/统计等）。
        /// </summary>
        public void OnEntityDeath(BattleContext context, string entityId)
        {
            context.DebugTraceService.TraceStateChange(entityId, "Alive", "Dead");
            context.EventBus.Publish(new EntityDeathEvent(entityId));
        }
    }

    /// <summary>
    /// 实体死亡事件（通过 EventBus 广播，供关卡/玩法/统计等订阅）。
    /// </summary>
    public readonly struct EntityDeathEvent
    {
        public string EntityId { get; }

        public EntityDeathEvent(string entityId)
        {
            EntityId = entityId;
        }
    }
}
