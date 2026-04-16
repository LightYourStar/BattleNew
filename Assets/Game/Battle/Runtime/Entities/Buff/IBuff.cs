using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Element;

namespace Game.Battle.Runtime.Entities.Buff
{
    /// <summary>
    /// Buff 接口：所有 Buff 的统一契约。
    /// </summary>
    public interface IBuff
    {
        string BuffId { get; }
        string OwnerId { get; }
        float Duration { get; }
        float Elapsed { get; }
        bool IsExpired { get; }

        void OnAdd(BattleContext context);
        void OnTick(BattleContext context, float deltaTime);
        void OnRemove(BattleContext context);

        /// <summary>
        /// 伤害修正钩子：当 OwnerId 为攻击方或受击方时被 DamageService 调用，
        /// 可在此修改 <see cref="DamageContext.FinalDamage"/> 或 <see cref="DamageContext.IsCancelled"/>。
        /// 默认空实现，需要修正伤害的 Buff 重写此方法。
        /// </summary>
        void ModifyDamage(DamageContext damageCtx) { }
    }
}
