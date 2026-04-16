using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Element;

namespace Game.Battle.Runtime.Entities.Trait
{
    /// <summary>
    /// 词条系统：持有 TraitRegistry，提供装备/卸载入口，
    /// 同时为 DamageService 提供伤害修正钩子。
    /// </summary>
    public sealed class TraitSystem
    {
        public TraitRegistry Registry { get; } = new();

        /// <summary>每逻辑帧更新（当前为占位，后续可用于周期性词条效果）。</summary>
        public void Tick(BattleContext context, float deltaTime)
        {
        }

        public void EquipTrait(BattleContext context, ITrait trait)
        {
            Registry.Equip(context, trait);
        }

        public void UnequipTrait(BattleContext context, ITrait trait)
        {
            Registry.Unequip(context, trait);
        }

        /// <summary>
        /// 遍历所有与本次伤害相关的词条（攻击方或受击方），调用其伤害修正钩子。
        /// DamageService 在计算最终伤害前调用此方法。
        /// </summary>
        public void ModifyDamage(DamageContext damageCtx)
        {
            var traits = Registry.ActiveTraits;
            for (int i = 0; i < traits.Count; i++)
            {
                ITrait trait = traits[i];
                if (trait.OwnerId == damageCtx.AttackerId || trait.OwnerId == damageCtx.TargetId)
                {
                    trait.ModifyDamage(damageCtx);
                }
            }
        }
    }
}
