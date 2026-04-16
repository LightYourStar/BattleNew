using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Buff;
using Game.Battle.Runtime.Entities.Element;

namespace Game.Battle.Hotfix.Buffs
{
    /// <summary>
    /// 示例热更 Buff：持续时间内为持有者提供百分比伤害加成。
    /// 演示如何重写 <see cref="IBuff.ModifyDamage"/> 实现数值修正。
    /// </summary>
    public sealed class DamageBoostBuff : BuffRuntime
    {
        /// <summary>伤害加成倍率，例如 0.2 = +20% 伤害。</summary>
        private readonly float _bonusRatio;

        public DamageBoostBuff(string ownerId, float duration, float bonusRatio = 0.2f)
            : base("buff_damage_boost", ownerId, duration)
        {
            _bonusRatio = bonusRatio;
        }

        /// <summary>
        /// 当持有者是攻击方时，将最终伤害乘以加成倍率。
        /// </summary>
        public void ModifyDamage(DamageContext damageCtx)
        {
            if (damageCtx.AttackerId == OwnerId)
            {
                damageCtx.FinalDamage *= 1f + _bonusRatio;
            }
        }
    }
}