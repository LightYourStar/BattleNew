using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Element;
using Game.Battle.Runtime.Entities.Hero;
using Game.Battle.Runtime.Entities.Trait;

namespace Game.Battle.Hotfix.Traits
{
    /// <summary>
    /// 示例热更词条：装备后增加英雄攻击范围，并通过 ModifyDamage 提升伤害输出。
    /// </summary>
    public sealed class DamageBoostTrait : ITrait
    {
        public string TraitId => "trait_damage_boost";
        public string OwnerId { get; }

        private readonly float _rangeBonus;
        private readonly float _damageRatio;

        public DamageBoostTrait(string ownerId, float rangeBonus = 2f, float damageRatio = 0.1f)
        {
            OwnerId = ownerId;
            _rangeBonus = rangeBonus;
            _damageRatio = damageRatio;
        }

        public void OnEquip(BattleContext context)
        {
            HeroEntity? hero = context.Registry.FindHero(OwnerId);
            if (hero != null)
            {
                hero.AttackRange += _rangeBonus;
            }
        }

        public void OnUnequip(BattleContext context)
        {
            HeroEntity? hero = context.Registry.FindHero(OwnerId);
            if (hero != null)
            {
                hero.AttackRange -= _rangeBonus;
            }
        }

        /// <summary>
        /// 当持有者是攻击方时，为最终伤害附加百分比加成。
        /// </summary>
        public void ModifyDamage(DamageContext damageCtx)
        {
            if (damageCtx.AttackerId == OwnerId)
            {
                damageCtx.FinalDamage *= 1f + _damageRatio;
            }
        }
    }
}
