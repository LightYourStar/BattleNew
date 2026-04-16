using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Element;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条接口：词条通过事件/数值修正/能力注册影响战斗。
    /// </summary>
    public interface ITrait
    {
        string TraitId { get; }
        string OwnerId { get; }

        void OnEquip(BattleContext context);
        void OnUnequip(BattleContext context);

        /// <summary>
        /// 伤害修正钩子：当 OwnerId 为攻击方或受击方时被 DamageService 调用，
        /// 可在此修改 <see cref="DamageContext.FinalDamage"/> 或 <see cref="DamageContext.IsCancelled"/>。
        /// 默认空实现，需要修正伤害的词条重写此方法。
        /// </summary>
        void ModifyDamage(DamageContext damageCtx) { }
    }
}
