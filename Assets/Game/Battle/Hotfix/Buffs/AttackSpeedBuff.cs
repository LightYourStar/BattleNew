using Game.Battle.Runtime.Core;
using Game.Battle.Runtime.Entities.Buff;
using Game.Battle.Runtime.Entities.Hero;

namespace Game.Battle.Hotfix.Buffs
{
    /// <summary>
    /// 示例热更 Buff：在持续时间内降低英雄攻击冷却（提高攻速）。
    /// </summary>
    public sealed class AttackSpeedBuff : BuffRuntime
    {
        private readonly float _cooldownReduction;

        public AttackSpeedBuff(string ownerId, float duration, float cooldownReduction)
            : base("buff_attack_speed", ownerId, duration)
        {
            _cooldownReduction = cooldownReduction;
        }

        public override void OnAdd(BattleContext context)
        {
            HeroEntity? hero = FindHero(context);
            if (hero?.CurrentWeapon != null)
            {
                hero.CurrentWeapon.CooldownSeconds -= _cooldownReduction;
                if (hero.CurrentWeapon.CooldownSeconds < 0.05f)
                {
                    hero.CurrentWeapon.CooldownSeconds = 0.05f;
                }
            }
        }

        public override void OnRemove(BattleContext context)
        {
            HeroEntity? hero = FindHero(context);
            if (hero?.CurrentWeapon != null)
            {
                hero.CurrentWeapon.CooldownSeconds += _cooldownReduction;
            }
        }

        private HeroEntity? FindHero(BattleContext context)
        {
            for (int i = 0; i < context.Registry.Heroes.Count; i++)
            {
                if (context.Registry.Heroes[i].Id == OwnerId)
                {
                    return context.Registry.Heroes[i];
                }
            }
            return null;
        }
    }
}
